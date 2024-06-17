using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModelSchedule = LeshaGay.Schedule;
using Schedule = LeshaGay.Data.Schedule;

namespace LeshaGay
{
    public class GeneticGenerator
    {
        private int populationSize;
        private int numberOfGenerations;
        private double mutationRate;
        private Random random;

        public List<LeshaGay.Data.Schedule> Population { get; set; }
        public Dictionary<Group, List<WorkloadTeachers>> Workload { get; set; }

        public GeneticGenerator(Dictionary<Group, List<WorkloadTeachers>> workload, int populationSize, int numberOfGenerations, double mutationRate)
        {
            this.Workload = workload;
            this.populationSize = populationSize;
            this.numberOfGenerations = numberOfGenerations;
            this.mutationRate = mutationRate;
            this.random = new Random((int)DateTime.Now.Ticks);
            this.Population = new List<LeshaGay.Data.Schedule>();
        }
        public List<Lesson> RunGeneticAlgorithm(int threads)
        {
            InitializePopulation();
            List<LeshaGay.Data.Schedule> newPopulation = new List<LeshaGay.Data.Schedule>();

            for (int i = 0; i < numberOfGenerations; i++)
            {               
                newPopulation = Population.OrderByDescending(s => s.Fitness).Take(populationSize / 10).ToList();

                List<Task> tasks = new List<Task>();
                for (int j = 0; j < threads; j++)
                {
                    int biggerTasks = (populationSize - newPopulation.Count) % threads;
                    int sizeTasks = (int)Math.Floor((double)((populationSize - newPopulation.Count) / threads));
                    if (j<biggerTasks)
                    {
                        Task bigTask = new Task(() =>
                        {
                            newPopulation.AddRange(GenPopulation(sizeTasks + 1));
                        });
                        bigTask.Start();
                        tasks.Add(bigTask);
                    }
                    else { 
                        Task littleTask = new Task(() =>
                        {
                            newPopulation.AddRange(GenPopulation(sizeTasks));
                        });
                        littleTask.Start();
                        tasks.Add(littleTask);
                    }
                }
                Task.WaitAll(tasks.ToArray());

                Population = new(newPopulation);
                Console.WriteLine($"Generation {i + 1}: generated");
                newPopulation.Clear();
            }

            return Population.OrderBy(p => p.Fitness).First().LessonPlans;

        }
        public List<LeshaGay.Data.Schedule> GenPopulation(int count)
        {
            List<LeshaGay.Data.Schedule> schedules = new List<LeshaGay.Data.Schedule>();
            for (int i = 0; i < count; i++)
            {
                LeshaGay.Data.Schedule child = Crossover(SelectParent(), SelectParent());
                Mutate(child);
                CalculateFitness(child);
                schedules.Add(child);
            }
            return schedules;
        }

        private LeshaGay.Data.Schedule SelectParent()
        {
            // Простой турнирный отбор
            LeshaGay.Data.Schedule parent1 = Population[random.Next(Population.Count)];
            LeshaGay.Data.Schedule parent2 = Population[random.Next(Population.Count)];

            return parent1.Fitness > parent2.Fitness ? parent1 : parent2;
        }

        public void InitializePopulation()
        {
            for (int i = 0; i < populationSize; i++)
            {
                LeshaGay.Data.Schedule schedule = CreateRandomSchedule();
                CalculateFitness(schedule);
                Population.Add(schedule);
            }
        }
        private LeshaGay.Data.Schedule CreateRandomSchedule()
        {
            LeshaGay.Data.Schedule schedule = new LeshaGay.Data.Schedule();

            foreach (var groupWorkload in Workload)
            {
                Group group = groupWorkload.Key;
                List<WorkloadTeachers> workloads = groupWorkload.Value;

                int currentDay = 1;
                int currentLessonNumber = 1;

                foreach (var workload in workloads)
                {
                    for (int hour = 0; hour < workload.HoursPerWeek; hour++)
                    {
                        if (currentLessonNumber > 5) // Если больше 5 уроков в день, переход на следующий день
                        {
                            currentLessonNumber = 1;
                            currentDay++;
                            if (currentDay > 6) currentDay = 1;
                        }

                        LessonGroup lessonGroup = new LessonGroup
                        {
                            Group = group,
                            Subject = workload.Subject,
                            LessonGroupTeachers = new List<LessonGroupTeacher>
                            {
                                new LessonGroupTeacher {
                                    Teacher = workload.Teachers.First(),
                                    Subgroup = 1 
                                    }
                            },
                            ScheduleType = "Plain"
                        };

                        Lesson lesson = new Lesson
                        {
                            LessonNumber = currentLessonNumber,
                            DayOfWeek = currentDay,
                            LessonGroup = lessonGroup,
                            Schedule = new Schedule()
                            {
                                AcademicYear = DateTime.Now.Year,
                                Semester = 1,
                                ScheduleStatus = new ScheduleStatus()
                                {
                                    Name = "чёрный"
                                }
                            },
                            Classroom = new Classroom()
                            {
                                Number = "1"
                            }// Нужно выбрать доступный класс
                        };

                        schedule.LessonPlans.Add(lesson);
                        currentLessonNumber++;
                    }
                }
            }

            return schedule;
        }

        private void CalculateFitness(LeshaGay.Data.Schedule schedule)
        {
            int fitness = 0;

            // Оценка окон между парами у преподавателей
            var teacherLessons = schedule.LessonPlans
                .SelectMany(l => l.LessonGroup.LessonGroupTeachers.Select(t => new { Teacher = t.Teacher, Lesson = l }))
                .GroupBy(x => x.Teacher.Id);

            foreach (var teacherLessonGroup in teacherLessons)
            {
                foreach (var dailyLessons in teacherLessonGroup.GroupBy(l => l.Lesson.DayOfWeek))
                {
                    var orderedLessons = dailyLessons.OrderBy(l => l.Lesson.LessonNumber).ToList();
                    for (int i = 1; i < orderedLessons.Count; i++)
                    {
                        int gap = orderedLessons[i].Lesson.LessonNumber - orderedLessons[i - 1].Lesson.LessonNumber;
                        if (gap > 1)
                        {
                            fitness -= (gap - 1); // Штраф за каждое окно
                        }
                    }
                }
            }

            // Оценка среднего количества пар в день у группы
            //foreach (var groupWorkload in Workload)
            //{
            //    Group group = groupWorkload.Key;
            //    var groupLessons = schedule.LessonPlans.Where(l => l.LessonGroup.Group.Id == group.Id);
            //    var dailyLessons = groupLessons.GroupBy(l => l.DayOfWeek).ToList();
            //    double averageLessonsPerDay = dailyLessons.Average(g => g.Count());
            //    fitness -= (int)Math.Round(Math.Abs(averageLessonsPerDay - 3)); // Штраф за отклонение от 3 пар в день
            //}

            // Оценка начала дня у группы
            foreach (var groupWorkload in Workload)
            {
                Group group = groupWorkload.Key;
                var groupLessons = schedule.LessonPlans.Where(l => l.LessonGroup.Group.Id == group.Id);
                var dailyLessons = groupLessons.GroupBy(l => l.DayOfWeek).ToList();
                foreach (var dayLessons in dailyLessons)
                {
                    var firstLesson = dayLessons.OrderBy(l => l.LessonNumber).FirstOrDefault();
                    if (firstLesson != null && firstLesson.LessonNumber > 1)
                    {
                        fitness -= (firstLesson.LessonNumber - 1); // Штраф за позднее начало дня
                    }
                }
            }

            schedule.Fitness = fitness;
        }

        public LeshaGay.Data.Schedule Crossover(LeshaGay.Data.Schedule parent1, LeshaGay.Data.Schedule parent2)
        {
            LeshaGay.Data.Schedule child = new LeshaGay.Data.Schedule();

            // Группируем уроки по LessonGroup
            var groupedLessonsParent1 = parent1.LessonPlans.GroupBy(l => l.LessonGroup).ToDictionary(g => g.Key, g => g.ToList());
            var groupedLessonsParent2 = parent2.LessonPlans.GroupBy(l => l.LessonGroup).ToDictionary(g => g.Key, g => g.ToList());

            // Перебираем каждую группу
            foreach (var group in groupedLessonsParent1.Keys.Union(groupedLessonsParent2.Keys))
            {
                var lessons1 = groupedLessonsParent1.ContainsKey(group) ? groupedLessonsParent1[group] : new List<Lesson>();
                var lessons2 = groupedLessonsParent2.ContainsKey(group) ? groupedLessonsParent2[group] : new List<Lesson>();

                // Создаем словарь для объединения уроков по (DayOfWeek, LessonNumber)
                var combinedLessons = new Dictionary<(int DayOfWeek, int LessonNumber), Lesson>();

                foreach (var lesson in lessons1)
                {
                    var key = (lesson.DayOfWeek, lesson.LessonNumber);
                    if (!combinedLessons.ContainsKey(key))
                    {
                        combinedLessons[key] = lesson;
                    }
                }

                foreach (var lesson in lessons2)
                {
                    var key = (lesson.DayOfWeek, lesson.LessonNumber);
                    if (!combinedLessons.ContainsKey(key))
                    {
                        combinedLessons[key] = lesson;
                    }
                }

                // Добавляем уроки в расписание ребенка с заменой по дням и номерам пар
                foreach (var key in combinedLessons.Keys)
                {
                    Lesson lesson = combinedLessons[key];
                    // Берем случайный урок из одного из родителей
                    Lesson selectedLesson = (random.Next(2) == 0) ?
                        lessons1.FirstOrDefault(l => l.DayOfWeek == key.DayOfWeek && l.LessonNumber == key.LessonNumber) :
                        lessons2.FirstOrDefault(l => l.DayOfWeek == key.DayOfWeek && l.LessonNumber == key.LessonNumber);

                    if (selectedLesson == null)
                    {
                        selectedLesson = lesson;
                    }

                    child.LessonPlans.Add(new Lesson
                    {
                        DayOfWeek = selectedLesson.DayOfWeek,
                        LessonNumber = selectedLesson.LessonNumber,
                        LessonGroup = selectedLesson.LessonGroup,
                        IsRemote = selectedLesson.IsRemote,
                        WeekOrderNumber = selectedLesson.WeekOrderNumber,
                        Classroom = selectedLesson.Classroom,
                        Schedule = selectedLesson.Schedule
                    });
                }
            }

            return child;
        }


        public void Mutate(LeshaGay.Data.Schedule schedule)
        {
            foreach (var lesson in schedule.LessonPlans)
            {
                if (random.Next(100) < mutationRate)
                {
                    int targetLessonNum = random.Next(1,6);
                    int targetWeekDay = random.Next(1,7);

                    Lesson? target = schedule.LessonPlans.Where(l => l.DayOfWeek == targetWeekDay && l.LessonNumber == targetLessonNum).FirstOrDefault();
                    if (target != null)
                    {
                        int tempLessonNum = lesson.LessonNumber;
                        int tempWeekDay = lesson.DayOfWeek;

                        lesson.DayOfWeek = targetWeekDay;
                        lesson.LessonNumber = targetLessonNum;

                        target.DayOfWeek = tempWeekDay;
                        target.LessonNumber = tempLessonNum;
                    }

                    while (schedule.LessonPlans.Where(l => l.DayOfWeek == targetWeekDay && l.LessonNumber == targetLessonNum +1 || l.LessonNumber == targetLessonNum - 1).Count()<=0)
                    {
                        targetLessonNum = random.Next(1, 6);
                        targetWeekDay = random.Next(1, 7);
                    }

                    lesson.DayOfWeek = targetWeekDay;
                    lesson.LessonNumber = targetLessonNum;
                }
            }
        }

        private bool IsValidSchedule(LeshaGay.Data.Schedule schedule)
        {
            var groupLessons = schedule.LessonPlans.GroupBy(l => l.LessonGroup.Group.Id);

            foreach (var group in groupLessons)
            {
                var lessons = group.OrderBy(l => l.DayOfWeek).ThenBy(l => l.LessonNumber).ToList();
                for (int i = 1; i < lessons.Count; i++)
                {
                    if (lessons[i].DayOfWeek == lessons[i - 1].DayOfWeek &&
                        lessons[i].LessonNumber != lessons[i - 1].LessonNumber + 1)
                    {
                        return false; // Обнаружено окно
                    }
                }
            }

            return true;
        }

    }
}

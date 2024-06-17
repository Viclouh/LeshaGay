using LeshaGay.Data;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


using ModelSchedule = LeshaGay.Schedule;
using Schedule = LeshaGay.Data.Schedule;
using System.Collections;

namespace LeshaGay
{
    public class GeneticGenerator
    {
        private Schedule _schedule = new Schedule()
        {
            AcademicYear = DateTime.Now.Year,
            Semester = 1,
            ScheduleStatus = new ScheduleStatus()
            {
                Name = "чёрный"
            }
        };
        private Classroom _classroom = new Classroom()
        {
            Number = "Нет"
        };

        private int populationSize;
        private int numberOfGenerations;
        private double mutationRate;
        private Random random;

        public List<LeshaGay.Data.Schedule> Population { get; set; }
        public Dictionary<Group, List<WorkloadTeachers>> Workload { get; set; }

        public GeneticGenerator(Dictionary<Group, List<WorkloadTeachers>> workload, int populationSize, int numberOfGenerations, double mutationRate)
        {
            this.Workload = new (workload);
            this.populationSize = populationSize;
            this.numberOfGenerations = numberOfGenerations;
            this.mutationRate = mutationRate;
            this.random = new Random((int)DateTime.Now.Ticks);
            this.Population = new List<LeshaGay.Data.Schedule>();
        }

        public List<Lesson> RunGeneticAlgorithm(int threads)
        {
            InitializePopulation();

            for (int i = 0; i < numberOfGenerations; i++)
            {
                BlockingCollection<Data.Schedule> newPopulation = new BlockingCollection<Data.Schedule>();

                foreach(var item in Population.OrderByDescending(s => s.Fitness).Take(populationSize / 10))
                {
                    newPopulation.Add(item);
                }

                List<Task> tasks = new List<Task>();
                for (int j = 0; j < threads; j++)
                {
                    int biggerTasks = (populationSize - newPopulation.Count) % threads;
                    int sizeTasks = (int)Math.Floor((double)((populationSize - newPopulation.Count) / threads));
                    if (j<biggerTasks)
                    {
                        Task bigTask = new Task(() =>
                        {
                            foreach (var item in GenPopulation(sizeTasks + 1))
                            { 
                                newPopulation.Add(item);
                            }
                        });
                        bigTask.Start();
                        tasks.Add(bigTask);
                    }
                    else { 
                        Task littleTask = new Task(() =>
                        {
                            foreach (var item in GenPopulation(sizeTasks))
                            {
                                newPopulation.Add(item);
                            }
                        });
                        littleTask.Start();
                        tasks.Add(littleTask);
                    }
                }
                Task.WaitAll(tasks.ToArray());

                Population = new((Data.Schedule[])newPopulation.ToArray());
                Console.WriteLine($"Generation {i + 1}: generated");
            }

            return Population.OrderBy(p => p.Fitness).First().LessonPlans;

        }

        public List<LeshaGay.Data.Schedule> GenPopulation(int count)
        {
            List<LeshaGay.Data.Schedule> schedules = new List<LeshaGay.Data.Schedule>();
            for (int i = 0; i < count; i++)
            {
                LeshaGay.Data.Schedule child = Crossover(SelectParent(), SelectParent());
                //LeshaGay.Data.Schedule child = SelectParent();
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
                int currentWeekNum = 0;

                foreach (var workload in workloads)
                {
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

                    for (double hour = 0; hour < workload.HoursPerWeek; hour+=2)
                    {
                        if (currentLessonNumber > 3) // Если больше 5 уроков в день, переход на следующий день
                        {

                            currentLessonNumber = 1;
                            currentDay++;
                            if (currentDay > 6)
                            {
                                currentDay = 1;
                                currentWeekNum++;
                            }

                        }                        

                        Lesson lesson = new Lesson
                        {
                            LessonNumber = currentLessonNumber,
                            DayOfWeek = currentDay,
                            LessonGroup = lessonGroup,
                            WeekOrderNumber = currentWeekNum,
                            Schedule = _schedule,
                            Classroom = _classroom
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

        public Lesson? SwapRandomLessons(List<Lesson> lessons1, List<Lesson> lessons2)
        {
            int randWeekday = random.Next(0, 7);
            int randLessonNum = random.Next(0, 6);


            Lesson lesson1 = lessons1.Where(l=> l.DayOfWeek == randWeekday && l.LessonNumber == randLessonNum).FirstOrDefault();
            Lesson lesson2 = lessons2.Where(l => l.DayOfWeek == randWeekday && l.LessonNumber == randLessonNum).FirstOrDefault();
            
            if (random.Next(0, 2) == 0)
            {
                if (lesson1 != null)
                {
                    lesson1 = lesson1.Clone();
                    (lesson1.LessonNumber, lesson1.DayOfWeek) = (randLessonNum, randWeekday);
                }
                return lesson1;
            }
            else
            {
                if (lesson2 != null)
                {
                    lesson2 = lesson2.Clone();
                    (lesson2.LessonNumber, lesson2.DayOfWeek) = (randLessonNum, randWeekday);
                }
                return lesson2;
            }
        }

        public LeshaGay.Data.Schedule Crossover(LeshaGay.Data.Schedule parent1, LeshaGay.Data.Schedule parent2)
        {
            var childSchedule = new LeshaGay.Data.Schedule();

            // Group lessons by LessonGroupId
            var groupedLessons1 = parent1.LessonPlans.GroupBy(lesson => lesson.LessonGroupId).ToDictionary(g => g.Key, g => g.ToList());
            var groupedLessons2 = parent2.LessonPlans.GroupBy(lesson => lesson.LessonGroupId).ToDictionary(g => g.Key, g => g.ToList());

            var allGroupIds = groupedLessons1.Keys.Union(groupedLessons2.Keys).Distinct();

            foreach (var groupId in allGroupIds)
            {
                if (groupedLessons1.TryGetValue(groupId, out var lessons1) && groupedLessons2.TryGetValue(groupId, out var lessons2))
                {
                    List<Lesson> selectedLessons = random.Next(2) ==0 ? lessons1 : lessons2;
                    childSchedule.LessonPlans.AddRange(selectedLessons);
                }       
            }

            return childSchedule;
        }

        public void Mutate(LeshaGay.Data.Schedule schedule)
        {
            var groupedLessons = schedule.LessonPlans.GroupBy(lesson => lesson.LessonGroup.Group).ToDictionary(g => g.Key, g => g.ToList());


            foreach (var group in groupedLessons.Keys)
            {
                if (groupedLessons.TryGetValue(group, out var lessons))
                {
                    for (int i = 0; i < lessons.Count; i++)
                    { 
                        if (random.Next(100) < mutationRate)
                        {
                            bool mutationFailed = true;
                            while (mutationFailed)
                            {
                                mutationFailed = false;
                                int randLessonNum1 = random.Next(1, 5);
                                int randWeekday1 = random.Next(1, 7);
                                int randLessonNum2 = random.Next(1, 5);
                                int randWeekday2 = random.Next(1, 7);

                                Lesson? lesson1 = lessons.Where(l => l.DayOfWeek == randWeekday1 && l.LessonNumber == randLessonNum1).FirstOrDefault();
                                Lesson? lesson2 = lessons.Where(l => l.DayOfWeek == randWeekday2 && l.LessonNumber == randLessonNum2).FirstOrDefault();



                                if (lesson2 != null &&
                                    lesson1 == null &&
                                    lessons.Where(l => l.DayOfWeek == randWeekday1 && (l.LessonNumber == randLessonNum1 + 1 || l.LessonNumber == randLessonNum1 - 1)).Count() > 0 &&
                                    lessons.Where(l => l.DayOfWeek == lesson2.DayOfWeek && (l.LessonNumber == lesson2.LessonNumber + 1 || l.LessonNumber == lesson2.LessonNumber - 1)).Count() < 2)
                                {
                                    (lesson2.DayOfWeek, lesson2.LessonNumber) = (randWeekday1, randLessonNum1);
                                }
                                else if (lesson1 != null &&
                                    lesson2 == null &&
                                    lessons.Where(l => l.DayOfWeek == randWeekday2 && (l.LessonNumber == randLessonNum2 + 1 || l.LessonNumber == randLessonNum2 - 1)).Count() > 0 &&
                                    lessons.Where(l => l.DayOfWeek == lesson1.DayOfWeek && (l.LessonNumber == lesson1.LessonNumber + 1 || l.LessonNumber == lesson1.LessonNumber - 1)).Count() < 2)
                                {
                                    (lesson1.DayOfWeek, lesson1.LessonNumber) = (randWeekday2, randLessonNum2);
                                }
                                else if (lesson1 != null && lesson2 != null)
                                {
                                    int temp = lesson1.DayOfWeek;
                                    lesson1.DayOfWeek = lesson2.DayOfWeek;
                                    lesson2.DayOfWeek = temp;
                                    temp = lesson1.LessonNumber;
                                    lesson1.LessonNumber = lesson2.LessonNumber;
                                    lesson2.LessonNumber = temp;
                                }
                                else
                                { 
                                    mutationFailed = true;
                                }
                            }
                        }
                    }
                }
            }


            //foreach (var lesson in schedule.LessonPlans)
            //{
            //    if (random.Next(100) < mutationRate)
            //    {
            //        int targetLessonNum = random.Next(1,6);
            //        int targetWeekDay = random.Next(1,7);

            //        Lesson? target = schedule.LessonPlans.Where(l => l.DayOfWeek == targetWeekDay && l.LessonNumber == targetLessonNum).FirstOrDefault();
            //        if (target != null)
            //        {
            //            int tempLessonNum = lesson.LessonNumber;
            //            int tempWeekDay = lesson.DayOfWeek;

            //            lesson.DayOfWeek = targetWeekDay;
            //            lesson.LessonNumber = targetLessonNum;

            //            target.DayOfWeek = tempWeekDay;
            //            target.LessonNumber = tempLessonNum;
            //        }

            //        while (schedule.LessonPlans.Where(l => l.DayOfWeek == targetWeekDay && l.LessonNumber == targetLessonNum +1 || l.LessonNumber == targetLessonNum - 1).Count()<=0)
            //        {
            //            targetLessonNum = random.Next(1, 6);
            //            targetWeekDay = random.Next(1, 7);
            //        }

            //        lesson.DayOfWeek = targetWeekDay;
            //        lesson.LessonNumber = targetLessonNum;
            //    }
            //}
        }
    }
}

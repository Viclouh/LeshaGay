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
                Console.WriteLine($"Generation {i + 1}: generated, max score: {Population.Max(s=>s.Fitness)}, avg prohod: {Population.OrderByDescending(s => s.Fitness).Take(populationSize / 10).Average(s=>s.Fitness)}");
            }
            return Population.OrderByDescending(p => p.Fitness).First().LessonPlans;

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
            var topSchedules = Population.OrderByDescending(s => s.Fitness).Take(populationSize / 10).ToList();
            // Простой турнирный отбор
            LeshaGay.Data.Schedule parent1 = topSchedules[random.Next(topSchedules.Count)];
            LeshaGay.Data.Schedule parent2 = topSchedules[random.Next(topSchedules.Count)];

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

            //Коллизии у преподов
            var lessonGroups = schedule.LessonPlans
               .SelectMany(l => l.LessonGroup.LessonGroupTeachers.Select(t => new { l.DayOfWeek, l.LessonNumber, t.Teacher }))
               .GroupBy(l => new { l.DayOfWeek, l.LessonNumber, l.Teacher })
               .Where(g => g.Count() > 1);

            fitness -= lessonGroups.Count()*2;

            // Оценка окон между парами у преподавателей
            //var teacherLessons = schedule.LessonPlans
            //    .GroupBy(x => x.LessonGroup.LessonGroupTeachers.First().Teacher);

            //foreach (var teacherLessonGroup in teacherLessons)
            //{
            //    foreach (var dailyLessons in teacherLessonGroup.GroupBy(l => l.DayOfWeek))
            //    {
            //        var orderedLessons = dailyLessons.OrderBy(l => l.LessonNumber).ToList();
            //        for (int i = 1; i < orderedLessons.Count; i++)
            //        {
            //            int gap = orderedLessons[i].LessonNumber - orderedLessons[i - 1].LessonNumber;
            //            if (gap > 1)
            //            {
            //                fitness -= (gap - 1); // Штраф за каждое окно
            //            }
            //        }
            //    }
            //}

            //Оценка распределённости дисциплин по разным дням
            var groupedLessons = schedule.LessonPlans.GroupBy(lp => lp.LessonGroup.Group).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var group in groupedLessons.Keys)
            {
                if (groupedLessons.TryGetValue(group, out var lessons))
                {
                    for (int i = 1; i < 7; i++)
                    {
                        var daySchedule = lessons.Where(l => l.DayOfWeek == i).ToList();
                        for (int j = 1; j < daySchedule.Count - 1; j++)
                        {
                            for (int k = j + 1; k < daySchedule.Count; k++)
                            {
                                if (daySchedule[j].LessonGroup.Equals(daySchedule[k].LessonGroup))
                                {
                                    fitness -= 1;
                                    daySchedule.RemoveAt(k);
                                }
                            }
                            daySchedule.RemoveAt(j);
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
        public static int CountTeacherCollisions(IEnumerable<Lesson> lessons)
        {
            // Словарь для хранения расписания уроков для каждого преподавателя
            var teacherSchedules = new Dictionary<int, List<(int DayOfWeek, int LessonNumber)>>();

            // Заполнение словаря данными о расписании уроков
            foreach (var lesson in lessons)
            {
                foreach (var lessonGroupTeacher in lesson.LessonGroup.LessonGroupTeachers)
                {
                    int teacherId = lessonGroupTeacher.TeacherId;
                    if (!teacherSchedules.ContainsKey(teacherId))
                    {
                        teacherSchedules[teacherId] = new List<(int, int)>();
                    }
                    teacherSchedules[teacherId].Add((lesson.DayOfWeek, lesson.LessonNumber));
                }
            }

            // Подсчет коллизий для каждого преподавателя
            int collisionCount = 0;
            foreach (var schedule in teacherSchedules.Values)
            {
                // Группировка расписания по дню недели и номеру урока
                var groupedSchedule = schedule.GroupBy(s => new { s.DayOfWeek, s.LessonNumber });
                // Коллизия возникает, если в группе больше одного урока
                collisionCount += groupedSchedule.Count(g => g.Count() > 1);
            }

            return collisionCount;
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
                    foreach (var lesson in selectedLessons)
                    {
                        childSchedule.LessonPlans.Add(lesson.Clone());
                    }
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

                                Lesson? lesson1 = lessons.Where(l => l.DayOfWeek == randWeekday1 && l.WeekOrderNumber == 0 && l.LessonNumber == randLessonNum1).FirstOrDefault();
                                Lesson? lesson2 = lessons.Where(l => l.DayOfWeek == randWeekday2 && l.WeekOrderNumber == 0 && l.LessonNumber == randLessonNum2).FirstOrDefault();

                                if (CanBeSwaped(lessons,randWeekday1,randWeekday2,randLessonNum1,randLessonNum2))
                                {
                                    if (lesson1!=null)
                                    {
                                        lesson1.DayOfWeek = randWeekday2;
                                        lesson1.LessonNumber = randLessonNum2;
                                    }
                                    if (lesson2!=null)
                                    {
                                        lesson2.DayOfWeek = randWeekday1;
                                        lesson2.LessonNumber = randLessonNum1;
                                    }
                                }

                                //if (lesson1 == null && lesson2 == null)
                                //{
                                //    mutationFailed = true;
                                //}
                                //else if (lesson1 != null && lesson2 != null)
                                //{
                                //    int temp = lesson1.DayOfWeek;
                                //    lesson1.DayOfWeek = lesson2.DayOfWeek;
                                //    lesson2.DayOfWeek = temp;
                                //    temp = lesson1.LessonNumber;
                                //    lesson1.LessonNumber = lesson2.LessonNumber;
                                //    lesson2.LessonNumber = temp;
                                //}
                                //else if (randWeekday1 == randWeekday2 && Math.Abs(randLessonNum2-randLessonNum1)==1)
                                //{
                                //    mutationFailed = true;
                                //}
                                //else if (lesson1 != null
                                //    && CanMove(lessons, lesson1)
                                //    && IsValidPlace(lessons, randWeekday2, randLessonNum2))
                                //{
                                //    lesson1.DayOfWeek = randWeekday2;
                                //    lesson1.LessonNumber = randLessonNum2;
                                //}
                                //else if (lesson2 != null
                                //    && CanMove(lessons, lesson2)
                                //    && IsValidPlace(lessons, randWeekday1, randLessonNum1))
                                //{
                                //    lesson2.DayOfWeek = randWeekday1;
                                //    lesson2.LessonNumber = randLessonNum1;
                                //}
                                //else
                                //{
                                //    mutationFailed = true;
                                //}


                                
                            }
                        }
                    }
                }
            }
        }

        public static bool CanMove(List<Lesson> lessons, Lesson lesson)
        {
            var weekdaySchedule = lessons.Where(l => l.DayOfWeek == lesson.DayOfWeek && l.WeekOrderNumber == 0);
            return !weekdaySchedule.Any(l=> l.LessonNumber < lesson.LessonNumber)
                || !weekdaySchedule.Any(l => l.LessonNumber > lesson.LessonNumber);
        }
        public static bool IsValidPlace(List<Lesson> lessons, int dayOfWeek, int lessonNumber)
        {
            var weekdaySchedule = lessons.Where(l => l.DayOfWeek == dayOfWeek && l.WeekOrderNumber == 0);
            return lessons.Any(l => l.LessonNumber == (lessonNumber + 1))
                || lessons.Any(l => l.LessonNumber == (lessonNumber - 1));
        }

        public static bool CanBeSwaped(List<Lesson> lessons, int weekday1, int weekday2, int lessonnum1, int lessonnum2)
        {
            var newlessons = new List<Lesson>();
            foreach (var item in lessons)
            {
                newlessons.Add(item.Clone());
            }
            Lesson? lesson1 = newlessons.Where(l => l.DayOfWeek == weekday1 && l.WeekOrderNumber == 0 && l.LessonNumber == lessonnum1).FirstOrDefault();
            Lesson? lesson2 = newlessons.Where(l => l.DayOfWeek == weekday2 && l.WeekOrderNumber == 0 && l.LessonNumber == lessonnum2).FirstOrDefault();


            if (lesson1 == null && lesson2 == null)
            {
                return false;
            }            
            if (lesson1!=null)
            {
                lesson1.DayOfWeek = weekday2;
                lesson1.LessonNumber = lessonnum2;
            }
            if (lesson2!= null)
            {
                lesson2.DayOfWeek = weekday1;
                lesson2.LessonNumber = lessonnum1;
            }

            var checkingList = newlessons.Where(l => l.DayOfWeek == weekday1).OrderBy(l=>l.LessonNumber).ToList();
            for (int i =0; i < checkingList.Count-1; i++)
            {
                if (checkingList[i].LessonNumber + 1 != checkingList[i+1].LessonNumber)
                {
                    return false;
                }
            }
            checkingList = newlessons.Where(l => l.DayOfWeek == weekday2).OrderBy(l => l.LessonNumber).ToList();
            for (int i = 0; i < checkingList.Count - 1; i++)
            {
                if (checkingList[i].LessonNumber + 1 != checkingList[i + 1].LessonNumber)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

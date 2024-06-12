
using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay
{
    public class GeneticAlgorithm
    {
        private List<Schedule> population;
        private Random random = new Random(DateTime.Now.Millisecond);
        private Dictionary<Group, List<WorkloadTeachers>> groupWorkloads;
        private double mutationRate;
        private double initialMutationRate;
        private int successfulCrossovers;
        private int unsuccessfulCrossovers;

        public GeneticAlgorithm(Dictionary<Group, List<WorkloadTeachers>> groupWorkloads, int populationSize, double mutationRate)
        {
            this.groupWorkloads = groupWorkloads;
            this.mutationRate = mutationRate;
            this.initialMutationRate = mutationRate;
            population = new List<Schedule>();

            // Инициализация популяции
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(GenerateRandomSchedule());
            }
        }

        public Schedule Run(int generations)
        {
            for (int generation = 0; generation < generations; generation++)
            {
                // Сброс счетчиков кроссоверов
                successfulCrossovers = 0;
                unsuccessfulCrossovers = 0;

                // Оценка приспособленности
                EvaluateFitness();

                // Отбор
                List<Schedule> newPopulation = Select();

                // Кроссовер и мутация
                for (int i = 0; i < newPopulation.Count; i += 2)
                {
                    if (i + 1 < newPopulation.Count)
                    {
                        if (Crossover(newPopulation[i], newPopulation[i + 1]))
                        {
                            successfulCrossovers++;
                        }
                        else
                        {
                            unsuccessfulCrossovers++;
                        }
                    }

                    Mutate(newPopulation[i]);
                    if (i + 1 < newPopulation.Count)
                    {
                        Mutate(newPopulation[i + 1]);
                    }
                }

                AdjustMutationRate();

                population = newPopulation;
                Console.WriteLine("Поколение {0} пройдено", generation);
            }

            // Возвращаем лучшее расписание
            return population.OrderBy(s => s.Fitness).First();
        }

        private void AdjustMutationRate()
        {
            if (unsuccessfulCrossovers > successfulCrossovers)
            {
                mutationRate = Math.Min(mutationRate * 1.1, 1.0); // Увеличение mutationRate на 10%, но не выше 1.0
            }
            else if (successfulCrossovers > unsuccessfulCrossovers)
            {
                mutationRate = Math.Max(mutationRate * 0.9, 0.01); // Уменьшение mutationRate на 10%, но не ниже 0.01
            }

            Console.WriteLine("Новый mutation rate: {0}", mutationRate);
        }

        private Schedule GenerateRandomSchedule()
        {
            Schedule schedule = new Schedule();
            foreach (var group in groupWorkloads.Keys)
            {
                int day = 1;
                int lessonNumber = 1;
                foreach (var workload in groupWorkloads[group])
                {
                    for (int i = 0; i < workload.HoursPerWeek; i++)
                    {
                        if (lessonNumber > 6)
                        {
                            lessonNumber = 1;
                            day++;
                            if (day > 6)
                            {
                                day = 1;
                            }
                        }

                        LessonPlan lessonPlan = new LessonPlan
                        {
                            Group = group,
                            Subject = workload.Subject,
                            Weekday = day,
                            LessonNumber = lessonNumber++
                        };

                        foreach (var teacher in workload.Teachers)
                        {
                            lessonPlan.LessonTeachers.Add(new LessonTeacher
                            {
                                Teacher = teacher,
                                IsGeneral = true // Или false, в зависимости от логики
                            });
                        }

                        schedule.LessonPlans.Add(lessonPlan);
                    }
                }
            }
            return schedule;
        }

        private Schedule GenerateGreedySchedule()
        {
            Schedule schedule = new Schedule();
            foreach (var group in groupWorkloads.Keys)
            {
                int day = 1;
                int lessonNumber = 1;
                int lessonsPerDay = 0;

                foreach (var workload in groupWorkloads[group])
                {
                    for (int i = 0; i < workload.HoursPerWeek; i++)
                    {
                        if (lessonsPerDay >= 3)
                        {
                            lessonNumber = 1;
                            day++;
                            lessonsPerDay = 0;
                            if (day > 6)
                            {
                                day = 1;
                            }
                        }

                        LessonPlan lessonPlan = new LessonPlan
                        {
                            Group = group,
                            Subject = workload.Subject,
                            Weekday = day,
                            LessonNumber = lessonNumber++
                        };

                        foreach (var teacher in workload.Teachers)
                        {
                            lessonPlan.LessonTeachers.Add(new LessonTeacher
                            {
                                Teacher = teacher,
                                IsGeneral = true // Или false, в зависимости от логики
                            });
                        }

                        schedule.LessonPlans.Add(lessonPlan);
                        lessonsPerDay++;
                    }
                }
            }
            return schedule;
        }

        private void EvaluateFitness()
        {
            List<Task> tasks = new List<Task>();
            foreach (var schedule in population)
            {
                var task = new Task(() => CalculateFitness(schedule));
                tasks.Add(task);
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());
        }

        private void CalculateFitness(Schedule schedule)
        {
            int conflicts = 0;

            // Dictionary to track teacher's occupied timeslots
            var teacherTimeSlots = new Dictionary<int, HashSet<(int Weekday, int LessonNumber)>>();

            // Check for teacher timeslot collisions
            foreach (var lessonPlan in schedule.LessonPlans)
            {
                foreach (var lessonTeacher in lessonPlan.LessonTeachers)
                {
                    if (!teacherTimeSlots.ContainsKey(lessonTeacher.Teacher.Id))
                    {
                        teacherTimeSlots[lessonTeacher.Teacher.Id] = new HashSet<(int, int)>();
                    }

                    var timeSlot = (lessonPlan.Weekday, lessonPlan.LessonNumber);
                    if (teacherTimeSlots[lessonTeacher.Teacher.Id].Contains(timeSlot))
                    {
                        conflicts += 10; // Conflict: teacher has another lesson at this time
                    }
                    else
                    {
                        teacherTimeSlots[lessonTeacher.Teacher.Id].Add(timeSlot);
                    }
                }
            }

            // Check for gaps between classes
            var groups = schedule.LessonPlans.GroupBy(l => l.Group);
            foreach (var group in groups)
            {
                // Группируем уроки по дням недели
                var lessonsByDay = group.GroupBy(l => l.Weekday);
                foreach (var day in lessonsByDay)
                {
                    // Сортируем уроки по времени начала
                    var sortedLessons = day.OrderBy(l => l.LessonNumber).ToList();
                    for (int i = 0; i < sortedLessons.Count - 1; i++)
                    {
                        if (sortedLessons[i].LessonNumber + 1 != sortedLessons[i + 1].LessonNumber)
                        {
                            conflicts++; // Conflict: gap between lessons
                        }
                    }
                }
            }

            // Fitness function adjustments
            foreach (var group in groups)
            {
                var lessonsByDay = group.GroupBy(l => l.Weekday);
                foreach (var day in lessonsByDay)
                {
                    var lessonCount = day.Count();
                    if (lessonCount != 3)
                    {
                        conflicts += Math.Abs(3 - lessonCount); // Conflict: deviation from 3 lessons per day
                    }

                    var sortedLessons = day.OrderBy(l => l.LessonNumber).ToList();
                    if (sortedLessons.First().LessonNumber > 1)
                    {
                        conflicts += sortedLessons.First().LessonNumber - 1; // Conflict: lessons not starting early
                    }
                }
            }

            schedule.Fitness = conflicts;
        }

        private List<Schedule> Select()
        {
            // Турнирный отбор
            List<Schedule> selected = new List<Schedule>();
            for (int i = 0; i < population.Count; i++)
            {
                List<Schedule> bestCandidates = new List<Schedule>(); // Список из 5 лучших кандидатов
                for (int j = 0; j < 5; j++)
                {
                    Schedule candidate = population[random.Next(population.Count)];
                    if (bestCandidates.Count < 5 || candidate.Fitness < bestCandidates.Max(c => c.Fitness))
                    {
                        if (bestCandidates.Count == 5)
                        {
                            // Удаляем худшее расписание из списка лучших кандидатов
                            bestCandidates.Remove(bestCandidates.First(c => c.Fitness == bestCandidates.Max(b => b.Fitness)));
                        }
                        bestCandidates.Add(candidate);
                    }
                }
                // Выбираем лучшее расписание из списка лучших кандидатов
                Schedule best = bestCandidates.OrderBy(c => c.Fitness).First();
                selected.Add(best);
            }
            return selected;
        }

        private bool Crossover(Schedule parent1, Schedule parent2)
        {
            int initialFitness1 = parent1.Fitness;
            int initialFitness2 = parent2.Fitness;

            // Одноточечный кроссовер
            int crossoverPoint = random.Next(parent1.LessonPlans.Count);
            for (int i = crossoverPoint; i < parent1.LessonPlans.Count; i++)
            {
                var temp = parent1.LessonPlans[i];
                parent1.LessonPlans[i] = parent2.LessonPlans[i];
                parent2.LessonPlans[i] = temp;
            }

            // Пересчитать приспособленность
            CalculateFitness(parent1);
            CalculateFitness(parent2);

            return parent1.Fitness < initialFitness1 && parent2.Fitness < initialFitness2;
        }

        private void Mutate(Schedule schedule)
        {
            // Поиск конфликтных ячеек
            var conflictSlots = new List<LessonPlan>();
            var teacherTimeSlots = new Dictionary<int, HashSet<(int Weekday, int LessonNumber)>>();

            foreach (var lessonPlan in schedule.LessonPlans)
            {
                foreach (var lessonTeacher in lessonPlan.LessonTeachers)
                {
                    if (!teacherTimeSlots.ContainsKey(lessonTeacher.Teacher.Id))
                    {
                        teacherTimeSlots[lessonTeacher.Teacher.Id] = new HashSet<(int, int)>();
                    }

                    var timeSlot = (lessonPlan.Weekday, lessonPlan.LessonNumber);
                    if (teacherTimeSlots[lessonTeacher.Teacher.Id].Contains(timeSlot))
                    {
                        conflictSlots.Add(lessonPlan);
                    }
                    else
                    {
                        teacherTimeSlots[lessonTeacher.Teacher.Id].Add(timeSlot);
                    }
                }
            }

            // Перемешивание конфликтных ячеек с другими случайными ячейками
            foreach (var conflict in conflictSlots)
            {
                int index = schedule.LessonPlans.IndexOf(conflict);
                int randomIndex = random.Next(schedule.LessonPlans.Count);

                // Обмен местами конфликтной ячейки с случайной ячейкой
                var temp = schedule.LessonPlans[randomIndex];
                schedule.LessonPlans[randomIndex] = conflict;
                schedule.LessonPlans[index] = temp;
            }
        }
    }
}

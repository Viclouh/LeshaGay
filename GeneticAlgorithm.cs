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
        private Random random = new Random();
        private Dictionary<Group, List<WorkloadTeachers>> groupWorkloads;

        public GeneticAlgorithm(Dictionary<Group, List<WorkloadTeachers>> groupWorkloads, int populationSize)
        {
            this.groupWorkloads = groupWorkloads;
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
                // Оценка приспособленности
                EvaluateFitness();

                // Отбор
                List<Schedule> newPopulation = Select();

                // Кроссовер и мутация
                for (int i = 0; i < newPopulation.Count; i += 2)
                {
                    if (i + 1 < newPopulation.Count)
                    {
                        Crossover(newPopulation[i], newPopulation[i + 1]);
                    }
                    Mutate(newPopulation[i]);
                }

                population = newPopulation;
            }

            // Возвращаем лучшее расписание
            return population.OrderBy(s => s.Fitness).First();
        }

        private Schedule GenerateRandomSchedule()
        {
            Schedule schedule = new Schedule();
            foreach (var group in groupWorkloads.Keys)
            {
                foreach (var workload in groupWorkloads[group])
                {
                    for (int i = 0; i < workload.HoursPerWeek; i++)
                    {
                        LessonPlan lessonPlan = new LessonPlan
                        {
                            Group = group,
                            Subject = workload.Subject,
                            WeekDay = random.Next(1, 6), // Понедельник-пятница
                            LessonNumber = random.Next(1, 7) // 1-6 пары
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

        private void EvaluateFitness()
        {
            foreach (var schedule in population)
            {
                int conflicts = 0;

                // Словарь для проверки коллизий преподавателей
                var teacherTimeSlots = new Dictionary<int, HashSet<(int WeekDay, int LessonNumber)>>();

                // Проверка коллизий преподавателей
                foreach (var lessonPlan in schedule.LessonPlans)
                {
                    foreach (var lessonTeacher in lessonPlan.LessonTeachers)
                    {
                        if (!teacherTimeSlots.ContainsKey(lessonTeacher.Teacher.Id))
                        {
                            teacherTimeSlots[lessonTeacher.Teacher.Id] = new HashSet<(int, int)>();
                        }

                        var timeSlot = (lessonPlan.WeekDay, lessonPlan.LessonNumber);
                        if (teacherTimeSlots[lessonTeacher.Teacher.Id].Contains(timeSlot))
                        {
                            conflicts++; // Конфликт: у преподавателя уже есть занятие в это время
                        }
                        else
                        {
                            teacherTimeSlots[lessonTeacher.Teacher.Id].Add(timeSlot);
                        }
                    }
                }

                // Проверка на окна между парами
                var groupTimeSlots = new Dictionary<int, List<(int WeekDay, int LessonNumber)>>();

                foreach (var lessonPlan in schedule.LessonPlans)
                {
                    if (!groupTimeSlots.ContainsKey(lessonPlan.Group.Id))
                    {
                        groupTimeSlots[lessonPlan.Group.Id] = new List<(int, int)>();
                    }

                    groupTimeSlots[lessonPlan.Group.Id].Add((lessonPlan.WeekDay, lessonPlan.LessonNumber));
                }

                foreach (var group in groupTimeSlots.Keys)
                {
                    var timeSlots = groupTimeSlots[group];
                    var groupedByDay = timeSlots.GroupBy(ts => ts.WeekDay);

                    foreach (var day in groupedByDay)
                    {
                        var lessonsInDay = day.OrderBy(ts => ts.LessonNumber).ToList();
                        for (int i = 1; i < lessonsInDay.Count; i++)
                        {
                            if (lessonsInDay[i].LessonNumber - lessonsInDay[i - 1].LessonNumber > 1)
                            {
                                conflicts++; // Окно между парами
                            }
                        }
                    }
                }

                // Присваиваем значение приспособленности
                schedule.Fitness = conflicts;
            }
        }

        private List<Schedule> Select()
        {
            // Турнирный отбор
            List<Schedule> selected = new List<Schedule>();
            for (int i = 0; i < population.Count; i++)
            {
                Schedule best = null;
                for (int j = 0; j < 5; j++)
                {
                    Schedule candidate = population[random.Next(population.Count)];
                    if (best == null || candidate.Fitness < best.Fitness)
                    {
                        best = candidate;
                    }
                }
                selected.Add(best);
            }
            return selected;
        }

        private void Crossover(Schedule parent1, Schedule parent2)
        {
            // Одноточечный кроссовер
            int crossoverPoint = random.Next(parent1.LessonPlans.Count);
            for (int i = crossoverPoint; i < parent1.LessonPlans.Count; i++)
            {
                var temp = parent1.LessonPlans[i];
                parent1.LessonPlans[i] = parent2.LessonPlans[i];
                parent2.LessonPlans[i] = temp;
            }
        }

        private void Mutate(Schedule schedule)
        {
            // Мутация путем смены местами двух случайных элементов
            int index1 = random.Next(schedule.LessonPlans.Count);
            int index2 = random.Next(schedule.LessonPlans.Count);
            var temp = schedule.LessonPlans[index1];
            schedule.LessonPlans[index1] = schedule.LessonPlans[index2];
            schedule.LessonPlans[index2] = temp;
        }
    }
}

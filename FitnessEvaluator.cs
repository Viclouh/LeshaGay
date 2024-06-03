using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay
{
    public class FitnessEvaluator
    {
        public static double Evaluate(Schedule schedule)
        {
            double fitness = 0;

            // 1. Проверка коллизий преподавателей
            fitness -= EvaluateTeacherCollisions(schedule);

            // 2. Оценка окон между парами для групп
            fitness -= EvaluateGroupGaps(schedule);

            return fitness;
        }
        private static int EvaluateTeacherCollisions(Schedule schedule)
        {
            int collisions = 0;

            // Создаем словарь для отслеживания занятости преподавателей
            var teacherSchedule = new Dictionary<Teacher, HashSet<Tuple<int, int, int>>>();

            foreach (var group in schedule.Timetable.Keys)
            {
                foreach (var week in schedule.Timetable[group].Keys)
                {
                    foreach (var day in schedule.Timetable[group][week].Keys)
                    {
                        foreach (var numPara in schedule.Timetable[group][week][day].Keys)
                        {
                            var session = schedule.Timetable[group][week][day][numPara];
                            foreach (var teacher in session.Item2)
                            {
                                if (!teacherSchedule.ContainsKey(teacher))
                                {
                                    teacherSchedule[teacher] = new HashSet<Tuple<int, int, int>>();
                                }
                                var timeSlot = new Tuple<int, int, int>(week, day, numPara);
                                if (teacherSchedule[teacher].Contains(timeSlot))
                                {
                                    collisions++;
                                }
                                else
                                {
                                    teacherSchedule[teacher].Add(timeSlot);
                                }
                            }
                        }
                    }
                }
            }

            return collisions;
        }

        private static int EvaluateGroupGaps(Schedule schedule)
        {
            int gaps = 0;

            foreach (var group in schedule.Timetable.Keys)
            {
                foreach (var week in schedule.Timetable[group].Keys)
                {
                    foreach (var day in schedule.Timetable[group][week].Keys)
                    {
                        var paraNumbers = schedule.Timetable[group][week][day].Keys.OrderBy(x => x).ToList();
                        for (int i = 1; i < paraNumbers.Count; i++)
                        {
                            if (paraNumbers[i] - paraNumbers[i - 1] > 1)
                            {
                                gaps += paraNumbers[i] - paraNumbers[i - 1] - 1;
                            }
                        }
                    }
                }
            }

            return gaps;
        }
    }
}

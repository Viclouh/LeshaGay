using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay
{
    public class Population
    {
        public List<Schedule> Schedules { get; set; }

        public Population(int size, Dictionary<Group, List<WorkloadTeachers>> workloadData, List<Classroom> classrooms, List<int> weeks, List<int> daysOfWeek, List<int> numParas)
        {
            Schedules = new List<Schedule>(size);
            for (int i = 0; i < size; i++)
            {
                Schedules.Add(GenerateRandomSchedule(workloadData, classrooms, weeks, daysOfWeek, numParas));
            }
        }

        private Schedule GenerateRandomSchedule(Dictionary<Group, List<WorkloadTeachers>> workloadData, List<Classroom> classrooms, List<int> weeks, List<int> daysOfWeek, List<int> numParas)
        {
            var schedule = new Schedule();
            var random = new Random();

            foreach (var group in workloadData.Keys)
            {
                schedule.Timetable[group] = new Dictionary<int, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>>();
                foreach (var workload in workloadData[group])
                {
                    int hoursAssigned = 0;
                    while (hoursAssigned < workload.HoursPerWeek)
                    {
                        int week = weeks[random.Next(weeks.Count)];
                        int day = daysOfWeek[random.Next(daysOfWeek.Count)];
                        int numPara = numParas[random.Next(numParas.Count)];
                        if (!schedule.Timetable[group].ContainsKey(week))
                        {
                            schedule.Timetable[group][week] = new Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>();
                        }
                        if (!schedule.Timetable[group][week].ContainsKey(day))
                        {
                            schedule.Timetable[group][week][day] = new Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>();
                        }
                        if (!schedule.Timetable[group][week][day].ContainsKey(numPara))
                        {
                            var classroom = classrooms[random.Next(classrooms.Count)];
                            schedule.Timetable[group][week][day][numPara] = new Tuple<Subject, List<Teacher>, Classroom?>(
                                workload.Subject,
                                workload.Teachers,
                                classroom
                            );
                            hoursAssigned++;
                        }
                    }
                }
            }

            return schedule;
        }
    }
}

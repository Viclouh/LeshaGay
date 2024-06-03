using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay
{
    class Scheduler
    {
        private List<TeacherEmployment> teacherEmployments = new();
        private List<Classroom> classroomEmployments = new();
        Schedule schedule = new Schedule();

        public Schedule GenerateSchedule(Dictionary<Group, List<WorkloadTeachers>> workload)
        {
            foreach (var classEntry in workload)
            {
                var currentClass = classEntry.Key;
                var workloadTeachers = classEntry.Value;

                var classSchedule = new Dictionary<int, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom>>>>();

                foreach (var subjectEntry in workloadTeachers)
                {
                    var subject = subjectEntry.Subject;
                    int countPara = (int)Math.Ceiling((double)subjectEntry.HoursPerWeek / 2);
                    int currentPara = 0;

                    for (int week = 0; week < 2; week++)
                    {
                        ResetTeacherAndClassroomAvailability();

                        for (int dayOfWeek = 1; dayOfWeek <= 6; dayOfWeek++)
                        {
                            for (int numPara = 1; numPara <= 6; numPara++)
                            {
                                if (classSchedule.ContainsKey(week) &&
                                    classSchedule[week].ContainsKey(dayOfWeek) &&
                                    classSchedule[week][dayOfWeek].ContainsKey(numPara))
                                    continue;

                                var employment = new TeacherEmployment
                                {
                                    Teacher = subjectEntry.Teachers.First(),
                                    NunWeek = week,
                                    DayOfWeek = dayOfWeek,
                                    NumPara = numPara,
                                };
                                bool availableTeacher = FindAvailableTeacherForSubject(employment);

                                if (availableTeacher && currentPara <= countPara)
                                {
                                    AddToSchedule(classSchedule, week, dayOfWeek, numPara, subject, subjectEntry.Teachers, new Classroom { Id = 1, Number = "111" });
                                    currentPara++;
                                    MarkTeacherAndClassroomAsBusy(employment);
                                }
                            }
                        }
                    }
                }

                schedule.Timetable[currentClass] = classSchedule;
            }

            return schedule;
        }

        void AddToSchedule(
            Dictionary<int, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom>>>> schedule,
            int week,
            int dayOfWeek,
            int numPara,
            Subject subject,
            List<Teacher> teachers,
            Classroom classroom)
        {
            if (!schedule.ContainsKey(week))
                schedule[week] = new Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom>>>();

            if (!schedule[week].ContainsKey(dayOfWeek))
                schedule[week][dayOfWeek] = new Dictionary<int, Tuple<Subject, List<Teacher>, Classroom>>();

            schedule[week][dayOfWeek][numPara] = Tuple.Create(subject, teachers, classroom);
        }

        private bool FindAvailableTeacherForSubject(TeacherEmployment employment)
        {
            return !teacherEmployments.Contains(employment);
        }

        private void ResetTeacherAndClassroomAvailability()
        {
            teacherEmployments.Clear();
            classroomEmployments.Clear();
        }

        private void MarkTeacherAndClassroomAsBusy(TeacherEmployment employment)
        {
            teacherEmployments.Add(employment);
        }
    }
}

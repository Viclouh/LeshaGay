using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay
{
    public class GeneticOperators
    {
        public static Schedule Crossover(Schedule parent1, Schedule parent2)
        {
            var child = new Schedule();
            var random = new Random(DateTime.Now.Second);
            foreach (var group in parent1.Timetable.Keys)
            {
                child.Timetable[group] = new Dictionary<int, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>>();
                foreach (var week in parent1.Timetable[group].Keys)
                {
                    child.Timetable[group][week] = new Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>();
                    foreach (var day in parent1.Timetable[group][week].Keys)
                    {
                        child.Timetable[group][week][day] = new Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>();
                        foreach (var numPara in parent1.Timetable[group][week][day].Keys)
                        {
                            var gene = (random.NextDouble() < 0.5) ? parent1.Timetable[group][week][day][numPara] : parent2.Timetable[group][week][day][numPara];
                            child.Timetable[group][week][day][numPara] = new Tuple<Subject, List<Teacher>, Classroom?>(
                                gene.Item1,
                                new List<Teacher>(gene.Item2),
                                gene.Item3);
                        }
                    }
                }
            }
            return child;
        }

        public static void Mutate(Schedule schedule, double mutationRate, List<Subject> subjects, List<Teacher> teachers, List<Classroom> classrooms)
        {
            var random = new Random();
            foreach (var group in schedule.Timetable.Keys)
            {
                foreach (var week in schedule.Timetable[group].Keys)
                {
                    foreach (var day in schedule.Timetable[group][week].Keys)
                    {
                        foreach (var numPara in schedule.Timetable[group][week][day].Keys)
                        {
                            var gene = schedule.Timetable[group][week][day][numPara];
                            if (random.NextDouble() < mutationRate)
                            {
                                var newSubject = subjects[random.Next(subjects.Count)];
                                gene = new Tuple<Subject, List<Teacher>, Classroom?>(newSubject, gene.Item2, gene.Item3);
                            }
                            if (random.NextDouble() < mutationRate)
                            {
                                var newTeachers = new List<Teacher> { teachers[random.Next(teachers.Count)] };
                                gene = new Tuple<Subject, List<Teacher>, Classroom?>(gene.Item1, newTeachers, gene.Item3);
                            }
                            if (random.NextDouble() < mutationRate)
                            {
                                var newClassroom = classrooms[random.Next(classrooms.Count)];
                                gene = new Tuple<Subject, List<Teacher>, Classroom?>(gene.Item1, gene.Item2, newClassroom);
                            }
                            schedule.Timetable[group][week][day][numPara] = gene;
                        }
                    }
                }
            }
        }
    }
}

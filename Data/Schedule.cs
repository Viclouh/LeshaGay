using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay.Data
{

    public class Schedule
    {
        public Dictionary<Group, Dictionary<int /*week*/, Dictionary<int  /*dayOfWeek*/, Dictionary<int /*numPara*/, Tuple<Subject, List<Teacher>, Classroom?>>>>> Timetable { get; set; }

        public Schedule()
        {
            Timetable = new Dictionary<Group, Dictionary<int, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>>>();
        }

        public Schedule Clone()
        {
            var clone = new Schedule();
            foreach (var group in Timetable.Keys)
            {
                clone.Timetable[group] = new Dictionary<int, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>>();
                foreach (var week in Timetable[group].Keys)
                {
                    clone.Timetable[group][week] = new Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>();
                    foreach (var day in Timetable[group][week].Keys)
                    {
                        clone.Timetable[group][week][day] = new Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>();
                        foreach (var numPara in Timetable[group][week][day].Keys)
                        {
                            var tuple = Timetable[group][week][day][numPara];
                            clone.Timetable[group][week][day][numPara] = new Tuple<Subject, List<Teacher>, Classroom?>(
                                tuple.Item1,
                                new List<Teacher>(tuple.Item2),
                                tuple.Item3);
                        }
                    }
                }
            }
            return clone;
        }
    }
}

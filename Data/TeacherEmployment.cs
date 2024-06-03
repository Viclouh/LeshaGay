using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay.Data
{
    public class TeacherEmployment
    {
        public Teacher Teacher { get; set; }
        public int NunWeek { get; set; }
        public int DayOfWeek { get; set; }
        public int NumPara { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not TeacherEmployment other)
                return false;

            return Teacher.Id == other.Teacher.Id &&
                   NunWeek == other.NunWeek &&
                   DayOfWeek == other.DayOfWeek &&
                   NumPara == other.NumPara;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Teacher.Id, NunWeek, DayOfWeek, NumPara);
        }
    }
}

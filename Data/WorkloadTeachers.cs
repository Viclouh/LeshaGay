
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay.Data
{
    public class WorkloadTeachers
    {
        public Subject Subject { get; set; }
        public List<Teacher> Teachers { get; set; }
        public int HoursPerWeek { get; set; }
    }
}

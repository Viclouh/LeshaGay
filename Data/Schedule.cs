
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
        public List<Lesson> LessonPlans { get; set; } = new List<Lesson>();
        public int Fitness { get; set; }
    }
}

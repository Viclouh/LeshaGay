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
        public List<LessonPlan> LessonPlans { get; set; } = new List<LessonPlan>();
        public int Fitness { get; set; }
    }
}

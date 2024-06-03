using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay.Data
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Shortname { get; set; }

        public virtual ICollection<LessonPlan> LessonPlans { get; set; }
    }
}

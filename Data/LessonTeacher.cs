using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay.Data
{
    public class LessonTeacher
    {
        [Key]
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int TeacherId { get; set; }
        public bool IsGeneral { get; set; }

        [ForeignKey("LessonId")]
        public virtual LessonPlan LessonPlan { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
    }
}

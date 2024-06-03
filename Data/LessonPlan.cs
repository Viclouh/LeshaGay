using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay.Data
{
    public class LessonPlan
    {
        public int Id { get; set; }
        public int LessonNumber { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public int AudienceId { get; set; }
        public Audience Audience { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public bool IsDistance { get; set; }
        public int WeekDay { get; set; }
        public int WeekNumber { get; set; }
        public List<LessonTeacher> LessonTeachers { get; set; } = new List<LessonTeacher>();
    }

}

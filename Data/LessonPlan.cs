using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("lesson_plan")]
    public class LessonPlan
    {
        public int Id { get; set; }
        public int LessonNumber { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public int? AudienceId { get; set; }
        public Audience Audience { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public bool isDistantce { get; set; }
        public int Weekday { get; set; }
        public int? WeekNumber { get; set; }
        public List<LessonTeacher> LessonTeachers { get; set; } = new();
    }
}

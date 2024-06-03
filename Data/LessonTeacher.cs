
namespace API.Models
{
    public class LessonTeacher
    {
        public int Id { get; set; }
        public LessonPlan Lesson { get; set; }
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public bool IsGeneral { get; set; }
    }
}

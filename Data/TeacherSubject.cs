using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class TeacherSubject
    {
        public int Id { get; set; }
        public Teacher Teacher { get; set; }
        public Subject Subject { get; set; }
    }
}

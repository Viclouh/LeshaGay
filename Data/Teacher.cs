using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay.Data
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }

        public virtual ICollection<LessonTeacher> LessonTeachers { get; set; }
    }
}

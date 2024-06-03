using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay.Data
{
    public class Audience
    {
        [Key]
        public int Id { get; set; }
        public string Number { get; set; }
        public int AudienceTypeId { get; set; }

        public virtual ICollection<LessonPlan> LessonPlans { get; set; }
    }
}

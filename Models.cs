using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeshaGay
{
    public class Change
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int LessonNumber { get; set; }
        public int ClassroomId { get; set; }
        public bool IsRemote { get; set; }
        public DateTime Date { get; set; }
        public bool IsCanceled { get; set; }
        public int LessonGroupId { get; set; }

        public Classroom Classroom { get; set; }
        public LessonGroup LessonGroup { get; set; }

        public Change Clone()
        {
            return new Change
            {
                LessonNumber = this.LessonNumber,
                IsRemote = this.IsRemote,
                Date = this.Date,
                IsCanceled = this.IsCanceled,
                Classroom = this.Classroom,
                LessonGroup = this.LessonGroup
            };
        }
    }

    public class Classroom
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Number { get; set; }
        public int? ClassroomTypeId { get; set; }

        public ClassroomType? ClassroomType { get; set; }

        public Classroom Clone()
        {
            return new Classroom
            {
                Number = this.Number,
                ClassroomTypeId = this.ClassroomTypeId,
                ClassroomType = this.ClassroomType?.Clone()
            };
        }
    }

    public class ClassroomType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        public ClassroomType Clone()
        {
            return new ClassroomType
            {
                Name = this.Name
            };
        }
    }

    public class Group
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Department { get; set; }
        public string GroupCode { get; set; }

        public Group Clone()
        {
            return new Group
            {
                Department = this.Department,
                GroupCode = this.GroupCode
            };
        }
    }

    public class Lesson
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int LessonNumber { get; set; }
        public int DayOfWeek { get; set; }
        public int ScheduleId { get; set; }
        public bool IsRemote { get; set; }
        public int WeekOrderNumber { get; set; }
        public int? ClassroomId { get; set; }
        public int LessonGroupId { get; set; }

        public Schedule Schedule { get; set; }
        public Classroom? Classroom { get; set; }
        public LessonGroup LessonGroup { get; set; }

        public Lesson Clone()
        {
            return new Lesson
            {
                LessonNumber = this.LessonNumber,
                DayOfWeek = this.DayOfWeek,
                WeekOrderNumber = this.WeekOrderNumber,
                IsRemote = this.IsRemote,
                Schedule = this.Schedule,
                Classroom = this.Classroom,
                LessonGroup = this.LessonGroup
            };
        }
        public override string ToString()
        {
            return $"DayOfWeek: {DayOfWeek}, lessonNum: {LessonNumber}";
        }
    }

    public class LessonGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int GroupId { get; set; }
        public string ScheduleType { get; set; }

        public Subject Subject { get; set; }
        public Group Group { get; set; }
        public ICollection<LessonGroupTeacher> LessonGroupTeachers { get; set; }

        public LessonGroup Clone()
        {
            return new LessonGroup
            {
                SubjectId = this.SubjectId,
                GroupId = this.GroupId,
                ScheduleType = this.ScheduleType,
                Subject = this.Subject?.Clone(),
                Group = this.Group?.Clone(),
                LessonGroupTeachers = this.LessonGroupTeachers?.Select(lgt => lgt.Clone()).ToList()
            };
        }
    }

    public class LessonGroupTeacher
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int LessonGroupId { get; set; }
        public int TeacherId { get; set; }
        public int Subgroup { get; set; }
        public bool IsMain { get; set; }

        public LessonGroup LessonGroup { get; set; }
        public Teacher Teacher { get; set; }

        public LessonGroupTeacher Clone()
        {
            return new LessonGroupTeacher
            {
                LessonGroupId = this.LessonGroupId,
                TeacherId = this.TeacherId,
                Subgroup = this.Subgroup,
                IsMain = this.IsMain,
                LessonGroup = this.LessonGroup?.Clone(),
                Teacher = this.Teacher?.Clone()
            };
        }
    }

    public class NotificationRequest
    {
        public string CustomKey { get; set; }
        public string Message { get; set; }
        public string TagKey { get; set; }
        public int TagValue { get; set; }

        public NotificationRequest Clone()
        {
            return new NotificationRequest
            {
                CustomKey = this.CustomKey,
                Message = this.Message,
                TagKey = this.TagKey,
                TagValue = this.TagValue
            };
        }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Role Clone()
        {
            return new Role
            {
                Name = this.Name
            };
        }
    }

    public class Schedule
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public int ScheduleStatusId { get; set; }

        public ScheduleStatus ScheduleStatus { get; set; }
        public ICollection<Lesson> Lessons { get; set; }

        public Schedule Clone()
        {
            return new Schedule
            {
                AcademicYear = this.AcademicYear,
                Semester = this.Semester,
                ScheduleStatusId = this.ScheduleStatusId,
                ScheduleStatus = this.ScheduleStatus?.Clone(),
                Lessons = this.Lessons?.Select(lesson => lesson.Clone()).ToList()
            };
        }
    }

    public class ScheduleStatus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        public ScheduleStatus Clone()
        {
            return new ScheduleStatus
            {
                Name = this.Name
            };
        }
    }

    public class Subject
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        public Subject Clone()
        {
            return new Subject
            {
                Name = this.Name,
                ShortName = this.ShortName
            };
        }
    }

    public class Teacher
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public Teacher Clone()
        {
            return new Teacher
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                MiddleName = this.MiddleName
            };
        }
    }

    public class TeacherSubject
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }

        public Subject Subject { get; set; }
        public Teacher Teacher { get; set; }

        public TeacherSubject Clone()
        {
            return new TeacherSubject
            {
                SubjectId = this.SubjectId,
                TeacherId = this.TeacherId,
                Subject = this.Subject?.Clone(),
                Teacher = this.Teacher?.Clone()
            };
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }

        public User Clone()
        {
            return new User
            {
                Email = this.Email,
                Name = this.Name
            };
        }
    }

    public class UserAuthData
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        public UserAuthData Clone()
        {
            return new UserAuthData
            {
                UserName = this.UserName,
                Password = this.Password,
                UserId = this.UserId,
                User = this.User?.Clone(),
                RoleId = this.RoleId,
                Role = this.Role?.Clone()
            };
        }
    }

    public class YearBegin
    {
        public int Id { get; set; }
        public DateTime DateStart { get; set; }

        public YearBegin Clone()
        {
            return new YearBegin
            {
                DateStart = this.DateStart
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeshaGay
{
    public class TeacherSubject
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }

        public Subject Subject { get; set; }
        public Teacher Teacher { get; set; }
    }
    public class Teacher
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public ICollection<TeacherSubject> TeacherSubjects { get; set; }
        public ICollection<LessonGroupTeacher> LessonGroupTeachers { get; set; }
    }
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        public ICollection<TeacherSubject> TeacherSubjects { get; set; }
        public ICollection<LessonGroup> LessonGroups { get; set; }
    }
    public class ScheduleStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Schedule> Schedules { get; set; }
    }
    public class Schedule
    {
        public int Id { get; set; }
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public int ScheduleStatusId { get; set; }

        public ScheduleStatus ScheduleStatus { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
    }
    public class LessonGroupTeacher
    {
        public int Id { get; set; }
        public int LessonGroupId { get; set; }
        public int TeacherId { get; set; }
        public int Subgroup { get; set; }
        public bool IsMain { get; set; }

        public LessonGroup LessonGroup { get; set; }
        public Teacher Teacher { get; set; }
    }
    public class LessonGroup
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int GroupId { get; set; }
        public int ScheduleType { get; set; }

        public Subject Subject { get; set; }
        public Group Group { get; set; }
        public ICollection<LessonGroupTeacher> LessonGroupTeachers { get; set; }
    }
    public class Lesson
    {
        public int Id { get; set; }
        public int LessonNumber { get; set; }
        public int ScheduleId { get; set; }
        public bool IsRemote { get; set; }
        public int DayOfWeek { get; set; }
        public int WeekOrderNumber { get; set; }
        public int ClassroomId { get; set; }
        public int LessonGroupId { get; set; }

        public Schedule Schedule { get; set; }
        public Classroom Classroom { get; set; }
        public LessonGroup LessonGroup { get; set; }
    }
    public class Group
    {
        public int Id { get; set; }
        public int Department { get; set; }
        public string GroupCode { get; set; }

        public ICollection<LessonGroup> LessonGroups { get; set; }
    }
    public class ClassroomType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Classroom> Classrooms { get; set; }
    }
    public class Classroom
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int? ClassroomTypeId { get; set; }

        public ClassroomType? ClassroomType { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<Change> Changes { get; set; }
    }
    public class Change
    {
        public int Id { get; set; }
        public int LessonNumber { get; set; }
        public int ClassroomId { get; set; }
        public bool IsRemote { get; set; }
        public DateTime Date { get; set; }
        public bool IsCanceled { get; set; }
        public int LessonGroupId { get; set; }

        public Classroom Classroom { get; set; }
        public LessonGroup LessonGroup { get; set; }
    }
    public class YearBegin
    {
        public int Id { get; set; }
        public DateTime DateStart { get; set; }
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
    }
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

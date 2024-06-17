using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay
{    
    public class MainContext : DbContext
    {
        private static MainContext _instance;

        public static MainContext Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = new MainContext();
                }
                return _instance;
            }
            
        }
        public DbSet<ScheduleStatus> ScheduleStatuses { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Change> Changes { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ClassroomType> ClassroomTypes { get; set; }
        public DbSet<LessonGroup> LessonGroups { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<TeacherSubject> TeacherSubjects { get; set; }
        public DbSet<LessonGroupTeacher> LessonGroupTeachers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Group> Groups { get; set; }

        public DbSet<YearBegin> YearBegin { get; set; }

        //Auth
        public DbSet<UserAuthData> UserAuthData { get; set; }
        public DbSet<User> Users { get; set; }

        public MainContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=hnt8.ru;Port=5432;Database=generationtesting;UserID=postgres;Password=_RasulkotV2");
        }
    }
}

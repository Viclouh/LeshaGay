using API.Models;

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
        public DbSet<AudienceType> AudienceType { get; set; }
        public DbSet<Audience> Audience { get; set; }

        public DbSet<Subject> Subject { get; set; }

        public DbSet<Speciality> Speciality { get; set; }
        public DbSet<Group> Group { get; set; }

        public DbSet<Teacher> Teacher { get; set; }
        public DbSet<TeacherSubject> TeacherSubject { get; set; }
        public DbSet<GroupTeacher> GroupTeacher { get; set; }

        public DbSet<LessonPlan> LessonPlan { get; set; }
        public DbSet<LessonTeacher> LessonTeacher { get; set; }

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
            optionsBuilder.UseNpgsql("Server=hnt8.ru;Port=5432;Database=temptesting;UserID=postgres;Password=_RasulkotV2");
        }
    }
}

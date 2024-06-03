using LeshaGay.Data;

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
        public DbSet<Teacher> Teachers { get; set; }

        public MainContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=hnt8.ru;Port=5432;Database=temptesting;UserID=postgres;Password=_RasulkotV2");
        }
    }
}

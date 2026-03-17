using Microsoft.EntityFrameworkCore;
using SmartRollCall.Api.Models;

namespace SmartRollCall.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<ClassGroup> ClassGroups { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    }
}
using AppointmentService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.API.Repositories
{
    public class AppointmentDbContext : DbContext
    {
        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<Schedules> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("host=host.docker.internal; port=5432; database=AppointmentService; Username=postgres; Password=ilia07ilia");
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SpecialistService.API.Models;

namespace SpecialistService.API.Repositories
{
    public class SpecialistDbContext : DbContext
    {
        public DbSet<Specialists> Specialists { get; set; }
        public DbSet<Skills> Skills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("host=localhost; port=5432; database=SpecialistService; Username=postgres; Password=ilia07ilia");
        }
    }
}

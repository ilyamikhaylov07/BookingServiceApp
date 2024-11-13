using Microsoft.EntityFrameworkCore;
using UserService.API.Models;

namespace UserService.API.Repositories
{
    public class UserDbContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<UserProfiles> UserProfiles { get; set; }
        public DbSet<Role> Roles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Specialist" },
                new Role { Id = 2, Name = "User" });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("host=localhost; port=5432; database=UserService; Username=postgres; Password=ilia07ilia");
        }
    }
}

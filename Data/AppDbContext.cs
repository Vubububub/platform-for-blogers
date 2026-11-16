using Microsoft.EntityFrameworkCore;
using platform_for_blogers.Models;

namespace platform_for_blogers.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Логин должен быть уникальным
            modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
        }
    }
}
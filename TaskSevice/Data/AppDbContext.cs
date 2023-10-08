using Microsoft.EntityFrameworkCore;
using TaskSevice.Models;

namespace TaskSevice.Data
{
    public class AppDbContext:DbContext
    {
        public DbSet<Tasks> Tasks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();   // создаем базу данных при первом обращении
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tasks>().HasData(
                    new Tasks { 
                        Id = 1, 
                        Title = "Test",
                        Description = "Test",
                        Executors="Me",
                        PlannedEffort = 0,
                        ActualEffort = 0,
                    }
            );
        }

        /*protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=tasks.db");

        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Task>()
                .HasMany(t => t.Subtasks)
                .WithOne(t => t.ParentTask)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении задачи, удалять и все её подзадачи
        }*/
    }
}

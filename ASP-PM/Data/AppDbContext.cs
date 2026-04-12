using Microsoft.EntityFrameworkCore;
using ASP_PM.Models;

namespace ASP_PM.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectDocument> ProjectDocuments { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasOne(p => p.ProjectManager)
            .WithMany()
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Executors)
            .WithMany(e => e.ExecutorOfProjects)
            .UsingEntity(j => j.ToTable("ProjectExecutors"));
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Infrastructure.Data
{
    /// <summary>
    /// The EF Core database context for managing entities.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<StrategicGoal> StrategicGoals => Set<StrategicGoal>();
        public DbSet<AnnualGoal> AnnualGoals => Set<AnnualGoal>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<WeeklyUpdate> WeeklyUpdates => Set<WeeklyUpdate>();
        public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            // Project configuration
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.Status);
                
            // Audit log configuration
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.EntityName);
                
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.EntityId);
                
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.Timestamp);
        }
    }
}
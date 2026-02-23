using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class MainDbContext : DbContext
    {
        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {
        }

        public DbSet<UsrMas> UserMas { get; set; }
        public DbSet<CompanyProject> DbCredentials { get; set; }
        public DbSet<AgentJob> AgentJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsrMas>().ToTable("UsrMas");
            modelBuilder.Entity<CompanyProject>().ToTable("CompanyProject");

            modelBuilder.Entity<AgentJob>(entity =>
            {
                entity.HasKey(e => e.JobId);
                entity.Property(e => e.JobId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.Status).HasDefaultValue(AgentJobStatus.Pending);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ExpiresAt).HasDefaultValueSql("DATEADD(MINUTE, 2, GETUTCDATE())");
                entity.HasIndex(e => new { e.CKy, e.Status, e.CreatedAt })
                      .HasDatabaseName("IX_AgentJobs_CKy_Status");
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.DAL.DbContext
{
    public class TaskAnalysisDbContext
        : Microsoft.EntityFrameworkCore.DbContext, IApplicationDbContext
    {
        public TaskAnalysisDbContext(DbContextOptions<TaskAnalysisDbContext> options)
            : base(options)
        {
        }

        public DbSet<DepartmentAiAnalysisResult> DepartmentAiAnalysisResults { get; set; }

        public DbSet<PersonAiAnalysisResult> PersonAiAnalysisResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DepartmentAiAnalysisResult>()
                .HasIndex(x => new { x.Directorate, x.Department })
                .IsUnique();

            modelBuilder.Entity<PersonAiAnalysisResult>()
                .HasIndex(x => x.SicilNo)
                .IsUnique();
        }
    }
}

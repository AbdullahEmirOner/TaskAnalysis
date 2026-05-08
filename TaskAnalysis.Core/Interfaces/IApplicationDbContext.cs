using Microsoft.EntityFrameworkCore;
using TaskAnalysis.Core.Entities;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<DepartmentAiAnalysisResult> DepartmentAiAnalysisResults { get; set; }

        DbSet<PersonAiAnalysisResult> PersonAiAnalysisResults { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        DbSet<DirectorateTaskAnalysisResult> DirectorateTaskAnalysisResults { get; set; }
    }
}
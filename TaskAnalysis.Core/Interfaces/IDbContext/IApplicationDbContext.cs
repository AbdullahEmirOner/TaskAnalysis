using Microsoft.EntityFrameworkCore;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Entities.RecordEntities;

namespace TaskAnalysis.Core.Interfaces.IDbContext
{
    public interface IApplicationDbContext
    {
        DbSet<DepartmentAiAnalysisResult> DepartmentAiAnalysisResults { get; set; }

        DbSet<PersonAiAnalysisResult> PersonAiAnalysisResults { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        DbSet<DirectorateTaskAnalysisResult> DirectorateTaskAnalysisResults { get; set; }
    }
}
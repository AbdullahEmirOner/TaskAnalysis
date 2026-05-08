using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAnalysis.Core.Entities
{
    public class DirectorateTaskAnalysisResult
    {
        public int Id { get; set; }

        public string Directorate { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string ResultJson { get; set; } = "{}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
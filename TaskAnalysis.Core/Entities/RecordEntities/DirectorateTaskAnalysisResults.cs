using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAnalysis.Core.Entities
{
    public class DirectorateTaskAnalysisResult
    {
        public int Id { get; set; }

        public string Directorate { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")] // nvarchar(max) ile 2 GB’a kadar metin saklanabilir.
        public string ResultJson { get; set; } = "{}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
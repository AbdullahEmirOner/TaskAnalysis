using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Entities
{
    public class DepartmentAiAnalysisResult 
   // Eski sürümden kalmış entity 11.05.2026, referanslarda eski kodların silinmemesinden kaynaklı !!!
    {
        public int Id { get; set; }

        public string Directorate { get; set; } = string.Empty;
        public string? Department { get; set; }

        public int RecordCount { get; set; }
        public int ChunkCount { get; set; }

        public string Task { get; set; } = string.Empty;
        public string BestSolution { get; set; } = string.Empty;
        public int AutomationRate { get; set; }
        public string Recommendation { get; set; } = string.Empty;

        public string ProjectIdeasJson { get; set; } = "[]";
        public string ResponsiblePeopleJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public class UniqueTaskDto
    {
        public string Task { get; set; } = string.Empty;

        public List<string> Departments { get; set; } = new();

        // Aşağıdakı proplar farklı DTO' ya alınabilir
        public string BestSolution { get; set; } = string.Empty; // AI / RPA / Hybrid / Other

        public int AutomationRate { get; set; }

        public string Recommendation { get; set; } = string.Empty;

        public string ProjectIdea { get; set; }

        public string SimilarProjectName { get; set; }

        public string SimilarProjectLink { get; set; }
    }
}


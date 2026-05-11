using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public class PersonTaskAiAnalysisDto
    {
        public string Task { get; set; } = string.Empty;

        public int AiAutomationRate { get; set; }

        public string BestSolution { get; set; } = string.Empty;

        public string Recommendation { get; set; } = string.Empty;

        public string ProjectIdea { get; set; } = string.Empty;
    }

}

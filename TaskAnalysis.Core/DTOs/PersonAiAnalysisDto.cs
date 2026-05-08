using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public class PersonAiAnalysisDto
    {
        public string SicilNo { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Birim { get; set; } = string.Empty;

        public string Mudurluk { get; set; } = string.Empty;

        public int TotalTaskCount { get; set; }

        public int AverageAiAutomationRate { get; set; }

        public string GeneralComment { get; set; } = string.Empty;

        public bool FromCache { get; set; }

        public List<PersonTaskAiAnalysisDto> TaskAnalyses { get; set; } = new();
    }
}

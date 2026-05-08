using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public class TaskAiAnalysisItemDto
    {
        public string Department { get; set; } = string.Empty;
        public string OriginalTask { get; set; } = string.Empty;
        public string TaskSummary { get; set; } = string.Empty;
        public int AiSupportRate { get; set; }
        public string ProjectIdea { get; set; } = string.Empty;
        public string SimilarProjectName { get; set; } = string.Empty;
        public string SimilarProjectLink { get; set; } = string.Empty;
    }
}

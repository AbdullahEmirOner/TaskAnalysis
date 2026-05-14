using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.DTOs.AIDTOs;

namespace TaskAnalysis.Core.DTOs.DepartmentDTOs
{
    public class TaskAiAnalysisItemDto
    {
        public string Department { get; set; } = string.Empty;

//----------------------------------------------------------------------------------
//----------------------- AI Dönüş Dto'su da aşağdaki proplar ----------------------
//----------------------------------------------------------------------------------
        public string OriginalTask { get; set; } = string.Empty;

        public string TaskSummary { get; set; } = string.Empty;

        public int AiSupportRate { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public string BestSolution { get; set; } = string.Empty;

        public List<ProjectIdeaDto> ProjectIdeas { get; set; } = new();

        public string SimilarProjectName { get; set; } = string.Empty;

        public string SimilarProjectLink { get; set; } = string.Empty;

        public List<ResponsiblePersonDto> ResponsiblePeople { get; set; } = new();

    }
}

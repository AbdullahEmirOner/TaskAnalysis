using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public class ProjectIdeaDto
    {
        public string ProjectIdea { get; init; } = string.Empty;
        public string SimilarProjectName { get; init; } = string.Empty;
        public string SimilarProjectLink { get; init; } = string.Empty;
    }
}

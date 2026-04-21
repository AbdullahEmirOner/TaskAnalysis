using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public record AiDepartmentAnalysisDto
    {
        public string Mudurluk { get; init; } = string.Empty;
        public List<AiAnalysisResponseDto> Analizler { get; init; } = new();
    }
}

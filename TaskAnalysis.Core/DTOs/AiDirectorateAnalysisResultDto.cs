using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public record AiDirectorateAnalysisResultDto
    {
        public string Direktorluk { get; init; } = string.Empty;
        public List<AiDepartmentAnalysisDto> DirektorlukAnalizleri { get; init; } = new();
    }
}

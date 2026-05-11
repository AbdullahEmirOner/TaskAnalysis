using TaskAnalysis.Core.DTOs.DepartmentDTOs;

namespace TaskAnalysis.Core.DTOs
{
    public class DirectorateTaskAnalysisDto
    {
        public string Directorate { get; set; } = string.Empty;

        public int DepartmentCount { get; set; }

        public int OriginalTaskCount { get; set; }

        public int UniqueTaskCount { get; set; }

        public List<DepartmentTaskAnalysisDto> Departments { get; set; } = new();
    }
}
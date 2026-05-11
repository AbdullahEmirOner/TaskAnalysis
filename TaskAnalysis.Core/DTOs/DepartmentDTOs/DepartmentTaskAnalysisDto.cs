namespace TaskAnalysis.Core.DTOs.DepartmentDTOs
{
    public class DepartmentTaskAnalysisDto
    {
        public string Department { get; set; } = string.Empty;
        public int OriginalTaskCount { get; set; }
        public int UniqueTaskCount { get; set; }

        public List<TaskAiAnalysisItemDto> Tasks { get; set; } = new();
    }
}
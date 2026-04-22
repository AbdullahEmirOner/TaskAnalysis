namespace TaskAnalysis.Core.DTOs
{
    public record AiDepartmentDto
    {
        public string Department { get; init; } = string.Empty;
        public List<AiTaskAnalysisDto> Analyses { get; init; } = new();
    }

}

namespace TaskAnalysis.Core.DTOs.AIDTOs
{
    public record AiDirectorateDto
    {
        public string Directorate { get; init; } = string.Empty;
        public List<AiDepartmentDto> Departments { get; init; } = new();
    }
}

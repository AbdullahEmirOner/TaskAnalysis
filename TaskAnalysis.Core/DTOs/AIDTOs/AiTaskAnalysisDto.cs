namespace TaskAnalysis.Core.DTOs.AIDTOs
{
    public record AiTaskAnalysisDto
    {
        public string Task { get; init; } = string.Empty;

        public string BestSolution { get; init; } = string.Empty;

        public int AutomationRate { get; init; }

        public string Recommendation { get; init; } = string.Empty;

        public List<ProjectIdeaDto> ProjectIdeas { get; set; } = new();

        public List<ResponsiblePersonDto> ResponsiblePeople { get; set; } = new();

    }
}

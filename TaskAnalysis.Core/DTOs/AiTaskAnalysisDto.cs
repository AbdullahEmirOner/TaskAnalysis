namespace TaskAnalysis.Core.DTOs
{
    public record AiTaskAnalysisDto
    {
        public string Task { get; init; } = string.Empty;
        public string BestSolution { get; init; } = string.Empty;
        public int AutomationRate { get; init; }
        public string Recommendation { get; init; } = string.Empty;
        public string ProjectIdea { get; init; } = string.Empty;
        public string SimilarProjectName { get; init; } = string.Empty;
        public string SimilarProjectLink { get; init; } = string.Empty;

        public List<ResponsiblePersonDto> ResponsiblePeople { get; init; } = new();

    }
}

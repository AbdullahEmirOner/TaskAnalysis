namespace TaskAnalysis.Core.DTOs
{
    public record AiTaskAnalysisDto
    {
        public string Task { get; init; } = string.Empty;
        public string AiSuitability { get; init; } = string.Empty;
        public int AutomationRate { get; init; }
        public int EstimatedWeeklyHours { get; init; }
        public string Recommendation { get; init; } = string.Empty;
    }

}

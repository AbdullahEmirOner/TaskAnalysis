namespace TaskAnalysis.Core.DTOs
{
    public class AiTaskAnalysisDto
    {
        public string Task { get; set; } = string.Empty;
        public string BestSolution { get; set; } = string.Empty;
        public int AutomationRate { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public string ProjectIdea { get; set; } = string.Empty;
        public string SimilarProjectName { get; set; } = string.Empty;
        public string SimilarProjectLink { get; set; } = string.Empty;

        public List<ResponsiblePersonDto> ResponsiblePeople { get; set; } = new();

    }
}

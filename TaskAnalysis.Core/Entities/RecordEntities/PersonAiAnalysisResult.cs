namespace TaskAnalysis.Core.Entities.RecordEntities
{
    public class PersonAiAnalysisResult
    {
        public int Id { get; set; }

        public string SicilNo { get; set; } = string.Empty;

        public string ResultJson { get; set; } = "{}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

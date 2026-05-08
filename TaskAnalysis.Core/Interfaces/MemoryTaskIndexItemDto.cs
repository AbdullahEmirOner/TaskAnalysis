namespace TaskAnalysis.Core.DTOs
{
    public class MemoryTaskIndexItemDto
    {
        public string Directorate { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string TaskText { get; set; } = string.Empty;

        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
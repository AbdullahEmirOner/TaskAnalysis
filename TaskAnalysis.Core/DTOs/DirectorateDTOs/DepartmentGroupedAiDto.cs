using TaskAnalysis.Core.DTOs.PersonDTOs;

namespace TaskAnalysis.Core.DTOs.DirectorateDTOs
{
    public class DepartmentGroupedAiDto
    {
        public string Mudurluk { get; set; } = string.Empty;
        public int PersonCount { get; set; }
        public int TotalTaskCount { get; set; }
        public int AverageAiAutomationRate { get; set; }
        public List<PersonAiAnalysisDto> Persons { get; set; } = new();
    }
}

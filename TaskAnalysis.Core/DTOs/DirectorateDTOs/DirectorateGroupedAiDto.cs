using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs.DirectorateDTOs
{
    public class DirectorateGroupedAiDto
    {
        public string Birim { get; set; } = string.Empty;
        public int PersonCount { get; set; }
        public int TotalTaskCount { get; set; }
        public int AverageAiAutomationRate { get; set; }
        public List<DepartmentGroupedAiDto> Mudurlukler { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public record ChatbotContextDto
    {
        public List<DirectorateSummaryDto> DirektorlukOzetleri { get; init; } = new();
    }
}

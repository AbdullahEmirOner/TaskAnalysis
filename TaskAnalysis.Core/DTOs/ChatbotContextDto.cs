using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public record ChatbotContextDto // Kuualımamamkta ilrde kullanıma alınması planlanıyor
    {
        public List<DirectorateSummaryDto> DirektorlukOzetleri { get; init; } = new();
    }
}

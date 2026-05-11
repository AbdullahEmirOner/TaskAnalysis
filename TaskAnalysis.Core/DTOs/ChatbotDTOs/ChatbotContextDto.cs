using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.DTOs.DirectorateDTOs;

namespace TaskAnalysis.Core.DTOs.ChatbotDTOs
{
    public record ChatbotContextDto // Kullanımamakta ilerde kullanıma alınması planlanıyor
    {
        public List<DirectorateSummaryDto> DirektorlukOzetleri { get; init; } = new();
    }
}

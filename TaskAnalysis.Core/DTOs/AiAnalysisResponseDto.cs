using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.Enums;

namespace TaskAnalysis.Core.DTOs
{
    public record AiAnalysisResponseDto
    {
        public string Gorev { get; init; } = string.Empty;
        public AiSuitablitityEnum AiUygunluk { get; init; }
        public int OtomasyonOrani { get; init; }

        public int TahminiSaatHaftalik { get; init; }
        public string ZamanAraligi { get; init; } = string.Empty;
        public string ZamanAciklama { get; init; } = string.Empty;

        public string Aciklama { get; init; } = string.Empty;
        public string Oneri { get; init;} = string.Empty;
    }
}

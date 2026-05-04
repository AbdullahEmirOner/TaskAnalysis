using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public record DepartmentSummaryDto // Kod içinde unique yapı sağlamak için aynı yazılmış veriler temizleniyor. AI eğitimine bundan kayaklı sorunlar oluşabilir.
    {
        public string Mudurluk { get; init; } = string.Empty;
        public int KayitSayisi { get; init; }
        public List<string> Amaclar { get; init; } = new();
        public List<string> Yetkinlikler { get; init; } = new();
        public List<string> AnaSorumluluklar { get; init; } = new();
        public List<string> AdSoyadlar { get; init; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public record DirectorateSummaryDto
    {

        public string Direktorluk { get; init; } = string.Empty;
        public int ToplamKayitSayisi { get; init; }
        public int MudurlukSayisi { get; init; }
        public List<DepartmentSummaryDto> Mudurlukler { get; init; } = new();
    }
}

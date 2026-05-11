using TaskAnalysis.Core.DTOs.DepartmentDTOs;

namespace TaskAnalysis.Core.DTOs.DirectorateDTOs
{
    public record DirectorateSummaryDto
    {
        public string Direktorluk { get; init; } = string.Empty;

        public int ToplamKayitSayisi { get; init; }

        public int MudurlukSayisi { get; init; }

        public List<DepartmentSummaryDto> Mudurlukler { get; init; } = new();
    }
}

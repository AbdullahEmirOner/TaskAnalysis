namespace TaskAnalysis.Core.DTOs.AIDTOs
{

    public record AiDepartmentDto // AIDTOs departmana toplu AI önerisinde bulunurken kullanılan yapı, güncel değil 11.05.2026 
    // Refransların bulunması yine bu mantıkla yazdığımız diğer kodların silinmemesinden kaynaklı, referansları temizleyip bu yapıyı da güncelleyebiliriz, şu an için sorun yok 11.05.2026
    {
        public string Department { get; init; } = string.Empty;
        public List<AiTaskAnalysisDto> Analyses { get; init; } = new();
    }
}

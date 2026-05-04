using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Interfaces;

public class ResponsiblePersonMatcherService : IResponsiblePersonMatcherService
{
    public List<ResponsiblePersonDto> FindResponsiblePeople(
        List<TaskRecord> records,
        string text,
        int take = 5)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<ResponsiblePersonDto>();

        var keywords = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return records
            .Where(x => !string.IsNullOrWhiteSpace(x.ad_soyad))
            .Select(x => new
            {
                Record = x,
                Score = keywords.Count(k =>
                    (x.AnaSorumluluk?.ToLower().Contains(k) ?? false) ||
                    (x.Amac?.ToLower().Contains(k) ?? false) ||
                    (x.Yetki?.ToLower().Contains(k) ?? false))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(take)
            .Select(x => new ResponsiblePersonDto
            {
                Name = x.Record.ad_soyad,
                Department = x.Record.Mudurluk ?? "",
                Reason = $"Eşleşme skoru: {x.Score}"
            })
            .ToList();
    }
}

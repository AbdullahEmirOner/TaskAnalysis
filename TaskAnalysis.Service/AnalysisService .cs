using System.Text;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.Services;

public class AnalysisService : IAnalysisService
{
    public List<DirectorateSummaryDto> BuildDirectoraterSummaries(List<TaskRecord> records)
    {
        if (records == null || records.Count == 0)
        {
            return new List<DirectorateSummaryDto>();
        }

        var result = records
        .GroupBy(x => x.Birim)
        .Select(dg => new DirectorateSummaryDto
        {
            Direktorluk = dg.Key,
            ToplamKayitSayisi = dg.Count(),
            MudurlukSayisi = dg
        .Select(x => x.Mudurluk)
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Count(),

            Mudurlukler = dg
        .GroupBy(x => x.Mudurluk)
        .Select(mg => new DepartmentSummaryDto
        {
            Mudurluk = mg.Key,
            KayitSayisi = mg.Count(),

            Amaclar = mg
        .Select(x => x.Amac)
        .Where(IsValidText)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x)
        .ToList(),

            Yetkinlikler = mg
        .Select(x => x.Yetki)
        .Where(IsValidText)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x)
        .ToList(),

            AnaSorumluluklar = mg
        .Select(x => x.AnaSorumluluk)
        .Where(IsValidText)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x)
        .ToList()
        })
        .Where(x => !string.IsNullOrWhiteSpace(x.Mudurluk))
        .OrderBy(x => x.Mudurluk)
        .ToList()
        })
        .Where(x => !string.IsNullOrWhiteSpace(x.Direktorluk))
        .OrderBy(x => x.Direktorluk)
        .ToList();

        return result;
        /*BuildDirectoraterSummaries
         Direktorluk → Grup anahtarı (Birim adı)

         ToplamKayitSayisi → O direktörlükteki toplam kayıt sayısı
         
         MudurlukSayisi → O direktörlükteki farklı müdürlüklerin sayısı
         
         Mudurlukler → Bir List<DepartmentSummaryDto>
         
         Her müdürlük için:
         
         Mudurluk → Müdürlük adı
         
         KayitSayisi → O müdürlükteki kayıt sayısı
         
         Amaclar → Distinct ve sıralı amaç listesi
         
         Yetkinlikler → Distinct ve sıralı yetki listesi
         
         AnaSorumluluklar → Distinct ve sıralı ana sorumluluk listesi
         */
    }

 /*   public ChatbotContextDto BuildChatbotContext(List<DirectorateSummaryDto> summaries)
    {
        return new ChatbotContextDto
        {
            DirektorlukOzetleri = summaries ?? new List<DirectorateSummaryDto>()
        };
    }*/

    private static bool IsValidText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public string BuildChatbotContext(List<DirectorateSummaryDto> summaries)
    {
        if (summaries == null || summaries.Count == 0)
            return "Analiz edilecek veri bulunamadı.";

        var sb = new StringBuilder();

        sb.AppendLine("Şirket görev analiz verileri:");
        sb.AppendLine();

        foreach (var directorate in summaries)
        {
            sb.AppendLine($"Direktörlük: {directorate.Direktorluk}");
            sb.AppendLine($"Toplam Kayıt Sayısı: {directorate.ToplamKayitSayisi}");
            sb.AppendLine($"Müdürlük Sayısı: {directorate.MudurlukSayisi}");
            sb.AppendLine();

            foreach (var department in directorate.Mudurlukler)
            {
                sb.AppendLine($"Müdürlük: {department.Mudurluk}");
                sb.AppendLine($"Kayıt Sayısı: {department.KayitSayisi}");

                sb.AppendLine("Amaçlar:");
                foreach (var amac in department.Amaclar)
                    sb.AppendLine($"- {amac}");

                sb.AppendLine("Yetkinlikler:");
                foreach (var yetkinlik in department.Yetkinlikler)
                    sb.AppendLine($"- {yetkinlik}");

                sb.AppendLine("Ana Sorumluluklar:");
                foreach (var sorumluluk in department.AnaSorumluluklar)
                    sb.AppendLine($"- {sorumluluk}");

                sb.AppendLine();
            }

            sb.AppendLine("--------------------------------");
        }

        return sb.ToString();
    }

    public List<UniqueTaskDto> BuildUniqueTask(List<DirectorateSummaryDto> summaries)
    {
        if (summaries == null || summaries.Count == 0)
            return new List<UniqueTaskDto>();

        var result = summaries
            .SelectMany(d => d.Mudurlukler)
            .SelectMany(m => m.AnaSorumluluklar.Select(task => new
            {
                Task = task,
                Department = m.Mudurluk
            }))
            .Where(x => !string.IsNullOrWhiteSpace(x.Task))
            .GroupBy(x => x.Task.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new UniqueTaskDto
            {
                Task = g.First().Task,
                Departments = g.Select(x => x.Department)
                               .Where(x => !string.IsNullOrWhiteSpace(x))
                               .Distinct(StringComparer.OrdinalIgnoreCase)
                               .OrderBy(x => x)
                               .ToList()
            })
            .OrderBy(x => x.Task)
            .ToList();

        return result;
    }

}

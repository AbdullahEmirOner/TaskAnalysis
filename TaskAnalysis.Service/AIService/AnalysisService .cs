using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Interfaces;
using TaskAnalysis.Service.Builders;

namespace TaskAnalysis.Service.AIService;

public class AnalysisService : IAnalysisService
{
    private readonly Dictionary<string, List<(string Text,float[] Vector)>> _vectorStore = new();
    private readonly ICsvReaderService _csvReaderService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IAiService _aiService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IRetrievalService _retrieval;
    private readonly IVectorDbService _vectorDb; 

    public AnalysisService(IRetrievalService retrieval ,IVectorDbService vectorDbService, IEmbeddingService embeddingService,
        ICsvReaderService csvReaderService, IAiService aiService, IConfiguration configuration, IMemoryCache cache)
    {
        _csvReaderService = csvReaderService;
        _aiService = aiService;
        _configuration = configuration;
        _cache = cache;
        _embeddingService = embeddingService;
        _vectorDb = vectorDbService;
        _retrieval= retrieval;
    }
   
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
        .Where(_retrieval.IsValidText)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x)
        .ToList(),

         AdSoyadlar = mg
            .Select(x => x.ad_soyad)
            .Where(_retrieval.IsValidText)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList(),

            Yetkinlikler = mg
        .Select(x => x.Yetki)
        .Where(_retrieval.IsValidText)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x)
        .ToList(),

            AnaSorumluluklar = mg
        .Select(x => x.AnaSorumluluk)
        .Where(_retrieval.IsValidText)
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
    }

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

    /*   public ChatbotContextDto BuildChatbotContext(List<DirectorateSummaryDto> summaries)
       { // Anlamsız bir kod silinecek 06.05.2026
           return new ChatbotContextDto
           {
               DirektorlukOzetleri = summaries ?? new List<DirectorateSummaryDto>()
           };
       }*/

    public string BuildChatbotContext(List<DirectorateSummaryDto> summaries)
    { /* BuildChatbotContext senin LLM’e vereceğin context stringini hazırlıyor.
       Bu sayede model, şirket görev analizini yaparken düzgün bir formatta veri görüyor.
       */
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
    { /* BuildUniqueTask
       Şirket görev özetlerinden (DirectorateSummaryDto) çıkarılan benzersiz görevleri (UniqueTaskDto) üretmeni sağlıyor.
       Yani aynı sorumluluk farklı müdürlüklerde geçse bile tek bir görev olarak listeleniyor
      */
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

   /* public List<TaskRecord> GetRelevantRecords(List<TaskRecord> records, string question, int maxCount = 50) // SearchAsync / SearchAllAsync  mantıksal benzerlik var düzeltilmeli
    { /* Elindeki TaskRecord listesi içinden bir soruya en uygun kayıtları seçiyor.
        Yani “keyword‑bazlı filtreleme ve sıralama” yapıyor
       
        if (records == null || records.Count == 0)
            return new List<TaskRecord>();

        if (string.IsNullOrWhiteSpace(question))
            return records.Take(maxCount).ToList();

        var keywords = question
        .ToLower()
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Where(x => x.Length > 2)
        .Distinct()
        .ToList();

        var scoredRecords = records
        .Select(r =>
        {
            var text = $"{r.Amac} {r.AnaSorumluluk} {r.Yetki}".ToLower();

            int score = keywords.Count(k => text.Contains(k));

            return new
            {
                Record = r,
                Score = score
            };
        })
        .Where(x => x.Score > 0)
        .OrderByDescending(x => x.Score)
        .Take(maxCount)
        .Select(x => x.Record)
        .ToList();

        if (scoredRecords.Count == 0)
            return records.Take(maxCount).ToList();

        return scoredRecords;
    }*/
    public async Task<string> AskQuestionAsync(ChatbotQuestionDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
            throw new Exception("Soru boş olamaz.");

        var queryEmbedding = await _embeddingService.CreateEmbeddingAsync(request.Question);

        List<string> chunks;

        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            var safeFileName = Path.GetFileName(request.FileName);

            chunks = await _vectorDb.SearchAsync(safeFileName, queryEmbedding, 1);
        }
        else
        {
            chunks = await _vectorDb.SearchAllAsync(queryEmbedding, 1);
        }

        if (chunks == null || chunks.Count == 0)
            return "Henüz indexlenmiş veri bulunamadı. Önce index-all-csv endpointini çalıştırın.";

        var context = string.Join("\n\n", chunks);

        var prompt = AiPromptBuilder.BuildChatbotPrompt(context, request.Question);

        var aiResponse = await _aiService.AnalyzeAsync(prompt);

        return aiResponse;
    }
}

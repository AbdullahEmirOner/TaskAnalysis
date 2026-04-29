using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Interfaces;
using TaskAnalysis.Service.Builders;

namespace TaskAnalysis.Service.Services;

public class AnalysisService : IAnalysisService
{
    private readonly Dictionary<string, List<(string Text, List<float> Vector)>> _vectorStore = new();
    private readonly ICsvReaderService _csvReaderService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IAiService _aiService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IVectorDbService _vectorDb;

    public AnalysisService(IVectorDbService vectorDbService, IEmbeddingService embeddingService, ICsvReaderService csvReaderService, IAiService aiService, IConfiguration configuration, IMemoryCache cache)
    {
        _csvReaderService = csvReaderService;
        _aiService = aiService;
        _configuration = configuration;
        _cache = cache;
        _embeddingService = embeddingService;
        _vectorDb = vectorDbService;
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

    public List<TaskRecord> GetRelevantRecords(List<TaskRecord> records, string question, int maxCount = 50)
    {
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
    }

    public async Task<object> AskQuestionAsync(ChatbotQuestionDto request)
    {
        var queryEmbedding = await _embeddingService.CreateEmbeddingAsync(request.Question);

        var relevantChunks = await _vectorDb.SearchAsync(queryEmbedding);

        var context = string.Join("\n\n", relevantChunks);

        var prompt = AiPromptBuilder.BuildChatbotPrompt(context, request.Question);

        var aiResponse = await _aiService.AnalyzeAsync(prompt);

        return new
        {
            question = request.Question,
            answer = aiResponse
        };
    }

    private static bool IsValidText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public List<string> ChunkRecords(List<TaskRecord> records)
    {
        return records.Select(r =>
        $"Direktörlük: {r.Mudurluk} | Birim: {r.Birim} | Amaç: {r.Amac} | Ana Sorumluluk: {r.AnaSorumluluk}"
        ).ToList();
    }
    //   SicilNo;Birim;Mudurluk;Amac;Yetki;AnaSorumluluk

    public async Task IndexCsvAsync(string fileName, List<TaskRecord> records)
    {
        var chunks = records.Select(r =>
            $"Direktörlük: {r.Mudurluk} | Birim: {r.Birim} | Amaç: {r.Amac} | Sorumluluk: {r.AnaSorumluluk}"
        ).ToList();

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingService.CreateEmbeddingAsync(chunk);
            await _vectorDb.InsertAsync(chunk, embedding);
        }
    }

    private double CosineSimilarity(float[] v1, float[] v2)
    {
        var dot = v1.Zip(v2, (a, b) => a * b).Sum();
        var mag1 = Math.Sqrt(v1.Sum(x => x * x));
        var mag2 = Math.Sqrt(v2.Sum(x => x * x));

        return dot / (mag1 * mag2 + 1e-8);
    }

    public async Task<List<string>> RetrieveRelevantChunks(string fileName, string question)
    {
        if (!_vectorStore.ContainsKey(fileName))
            return new List<string>();

        var questionEmbedding = await _embeddingService.CreateEmbeddingAsync(question);

        var scored = _vectorStore[fileName]
            .Select(v => new
            {
                Text = v.Text,
                Score = CosineSimilarity(v.Vector, questionEmbedding)
            })
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => x.Text)
            .ToList();

        return scored;
    }
}

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

         AdSoyadlar = mg
            .Select(x => x.ad_soyad)
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
       {
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

    public List<TaskRecord> GetRelevantRecords(List<TaskRecord> records, string question, int maxCount = 50) // SearchAsync / SearchAllAsync  mantıksal benzerlik var düzeltilmeli
    { /* Elindeki TaskRecord listesi içinden bir soruya en uygun kayıtları seçiyor.
        Yani “keyword‑bazlı filtreleme ve sıralama” yapıyor
       */
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

    public async Task<object> IndexAllCsvAsync()
    {
        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            throw new Exception("CSV klasör yolu tanımlı değil.");

        if (!Directory.Exists(folderPath))
            throw new Exception("CSV klasörü bulunamadı.");

        var files = Directory.GetFiles(folderPath, "*.csv");

        var results = new List<object>();

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);

            try
            {
                var result = await IndexCsvAsync(fileName);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    fileName,
                    indexed = false,
                    error = ex.Message
                });
            }
        }

        return new
        {
            indexedFileCount = results.Count(x =>
                x.GetType().GetProperty("indexed")?.GetValue(x)?.Equals(true) == true),
            totalFileCount = files.Length,
            files = results,
            message = "CSV indexleme işlemi tamamlandı."
        };
    }

    public async Task<string> AskQuestionAsync(ChatbotQuestionDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
            throw new Exception("Soru boş olamaz.");

        var queryEmbedding = await _embeddingService.CreateEmbeddingAsync(request.Question);

        List<string> chunks;

        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            var safeFileName = Path.GetFileName(request.FileName);

            chunks = await _vectorDb.SearchAsync(safeFileName, queryEmbedding, 3);
        }
        else
        {
            chunks = await _vectorDb.SearchAllAsync(queryEmbedding, 3);
        }

        if (chunks == null || chunks.Count == 0)
            return "Henüz indexlenmiş veri bulunamadı. Önce index-all-csv endpointini çalıştırın.";

        var context = string.Join("\n\n", chunks);

        var prompt = AiPromptBuilder.BuildChatbotPrompt(context, request.Question);

        var aiResponse = await _aiService.AnalyzeAsync(prompt);

        return aiResponse;
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
    public async Task<object> IndexCsvAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new Exception("CSV dosya adı boş olamaz.");

        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            throw new Exception("CSV klasör yolu tanımlı değil.");

        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(folderPath, safeFileName);

        if (!File.Exists(filePath))
            throw new Exception("CSV dosyası bulunamadı.");

        var records = _csvReaderService.ReadCsv(filePath);

        if (records == null || records.Count == 0)
            throw new Exception("CSV okundu ama kayıt bulunamadı.");

        var chunks = CreateChunks(records, 20);

        _vectorDb.Clear(safeFileName);

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingService.CreateEmbeddingAsync(chunk);
            await _vectorDb.InsertAsync(safeFileName, chunk, embedding);
        }

        return new
        {
            fileName = safeFileName,
            indexed = true,
            recordCount = records.Count,
            chunkCount = chunks.Count,
            message = "CSV başarıyla memory vector store içine indexlendi."
        };
    }

    private List<string> CreateChunks(List<TaskRecord> records, int chunkSize = 20)
    {
        var chunks = new List<string>();

        for (int i = 0; i < records.Count; i += chunkSize)
        {
            var group = records.Skip(i).Take(chunkSize).ToList();

            var chunk = string.Join("\n", group.Select((r, index) =>
                $"Kayıt: {i + index + 1} | " +
                $"Müdürlük: {r.Mudurluk} | " +
                $"Birim: {r.Birim} | " +
                $"Amaç: {r.Amac} | " +
                $"Yetki: {r.Yetki} | " +
                $"Ana Sorumluluk: {r.AnaSorumluluk}"
            ));

            chunks.Add(chunk);
        }

        return chunks;
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
                v.Text,
                Score = CosineSimilarity(v.Vector, questionEmbedding)
            })
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => x.Text)
            .ToList();

        return scored;
    }

}

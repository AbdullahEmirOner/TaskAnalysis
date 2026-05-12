using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.DTOs.ChatbotDTOs;
using TaskAnalysis.Core.DTOs.DepartmentDTOs;
using TaskAnalysis.Core.DTOs.DirectorateDTOs;
using TaskAnalysis.Core.DTOs.PersonDTOs;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Entities.CSVEntities;
using TaskAnalysis.Core.Entities.RecordEntities;
using TaskAnalysis.Core.Interfaces;
using TaskAnalysis.Core.Interfaces.IAIService;
using TaskAnalysis.Core.Interfaces.ICsvReader;
using TaskAnalysis.Core.Interfaces.IDbContext;
using TaskAnalysis.Core.Interfaces.IRAG;
using TaskAnalysis.Service.Builders;
using TaskAnalysis.Service.Helpers;

namespace TaskAnalysis.Service.AIService;

public class AnalysisService : IAnalysisService
{
    private readonly Dictionary<string, List<(string Text,float[] Vector)>> _vectorStore = new();
    private readonly ICsvReaderService _csvReaderService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IEmbeddingHelperService _embeddingHelperService;
    private readonly IAiService _aiService;
    private readonly IMemoryCache _cache;
    private readonly IParseHeleprService _parseHeleprService;
    private readonly IConfiguration _configuration;
    private readonly IRetrievalService _retrieval;
    private readonly IVectorDbService _vectorDb;
    private readonly IApplicationDbContext _context;
    private readonly ITaskExtractionService _taskExtractionService;

    public AnalysisService(IRetrievalService retrieval ,IVectorDbService vectorDbService, IEmbeddingService embeddingService, ITaskExtractionService taskExtractionService,
        ICsvReaderService csvReaderService, IEmbeddingHelperService embeddingHelperService, IAiService aiService, IConfiguration configuration, IMemoryCache cache, IApplicationDbContext context, IParseHeleprService parseHeleprService)
    {
        _csvReaderService = csvReaderService;
        _embeddingHelperService = embeddingHelperService;   
        _aiService = aiService;
        _configuration = configuration;
        _cache = cache;
        _embeddingService = embeddingService;
        _vectorDb = vectorDbService;
        _retrieval= retrieval;
        _context =context;
        _taskExtractionService = taskExtractionService;
        _parseHeleprService = parseHeleprService;
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

    public async Task<PersonAiAnalysisDto> AnalyzePersonBySicilNoAsync(string sicilNo)
    {
        if (string.IsNullOrWhiteSpace(sicilNo))
            throw new ArgumentException("Sicil numarası boş olamaz.");

        sicilNo = sicilNo.Trim();

        var existingDbResult = await _context.PersonAiAnalysisResults
            .FirstOrDefaultAsync(x => x.SicilNo == sicilNo);

        if (existingDbResult != null)
        {
            var cachedPersonResult = JsonSerializer.Deserialize<PersonAiAnalysisDto>(
                existingDbResult.ResultJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (cachedPersonResult == null)
                return new PersonAiAnalysisDto();

            cachedPersonResult.FromCache = true;

            return cachedPersonResult;
        }

        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            throw new Exception("CsvSettings:FolderPath appsettings.json içinde bulunamadı.");

        var allRecords = _csvReaderService.ReadAllCsv(folderPath);

        var personRecords = allRecords
            .Where(x => x.SicilNo == sicilNo)
            .ToList();

        if (!personRecords.Any())
        {
            return new PersonAiAnalysisDto
            {
                SicilNo = sicilNo,
                GeneralComment = "Bu sicil numarasına ait görev kaydı bulunamadı.",
                FromCache = false
            };
        }

        var firstRecord = personRecords.First();

        var fullName = firstRecord.ad_soyad;
        var birim = firstRecord.Birim;
        var mudurluk = firstRecord.Mudurluk;

        var personContextChunks = personRecords
            .Select(x =>
                $"SicilNo: {x.SicilNo}\n" +
                $"Ad Soyad: {x.ad_soyad}\n" +
                $"Birim: {x.Birim}\n" +
                $"Müdürlük: {x.Mudurluk}\n" +
                $"Amaç: {x.Amac}\n" +
                $"Yetki: {x.Yetki}\n" +
                $"Ana Sorumluluk: {x.AnaSorumluluk}")
            .ToList();

        var analysisQuestion = "Bu çalışanın görevlerinin AI ile yapılabilirlik oranını analiz et. " + "Her görev için AI otomasyon yüzdesi, çözüm tipi, öneri ve proje fikri üret.";

        var questionEmbedding = await _embeddingService.CreateEmbeddingAsync(analysisQuestion);

        var scoredChunks = new List<(string Text, double Score)>();

        foreach (var chunk in personContextChunks)
        {
            var chunkEmbedding = await _embeddingService.CreateEmbeddingAsync(chunk);

            var score = _vectorDb.CosineSimilarity(questionEmbedding, chunkEmbedding);

            scoredChunks.Add((chunk, score));
        }

        var relevantChunks = scoredChunks
            .OrderByDescending(x => x.Score)
            .Take(8)
            .Select(x => x.Text)
            .ToList();

        var prompt = AiPromptBuilder.BuildPersonAiAnalysisPrompt( sicilNo, fullName, birim, mudurluk, relevantChunks);

        var aiResponse = await _aiService.AnalyzeAsync(prompt);

        var result = _parseHeleprService.ParsePersonAiAnalysis(aiResponse);

        result.SicilNo = sicilNo;
        result.FullName = fullName;
        result.Birim = birim;
        result.Mudurluk = mudurluk;
        result.TotalTaskCount = personRecords.Count;

        if (result.TaskAnalyses != null && result.TaskAnalyses.Any())
        {
            result.AverageAiAutomationRate = Convert.ToInt32(
                result.TaskAnalyses.Average(x => x.AiAutomationRate));
        }

        result.FromCache = false;

        var entity = new PersonAiAnalysisResult
        {
            SicilNo = sicilNo,
            ResultJson = JsonSerializer.Serialize(result),
            CreatedAt = DateTime.UtcNow
        };

        _context.PersonAiAnalysisResults.Add(entity);
        await _context.SaveChangesAsync();

        return result;
    }

// -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
   
    public async Task<DirectorateTaskAnalysisDto> AnalyzeDirectorateTasksWithMemoryIndexAsync(
        string directorate,
        int chunkSize = 200)
    {
        if (string.IsNullOrWhiteSpace(directorate))
            throw new ArgumentException("Direktörlük boş olamaz.");

        directorate = directorate.Trim();

        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            throw new Exception("CSV klasör yolu bulunamadı.");

        var allRecords = _csvReaderService.ReadAllCsv(folderPath);

        var directorateRecords = allRecords
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Birim) &&
                x.Birim.Equals(directorate, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!directorateRecords.Any())
        {
            return new DirectorateTaskAnalysisDto
            {
                Directorate = directorate
            };
        }

        var result = new DirectorateTaskAnalysisDto
        {
            Directorate = directorate
        };

        var groupedDepartments = directorateRecords
            .Where(x => !string.IsNullOrWhiteSpace(x.Mudurluk))
            .GroupBy(x => x.Mudurluk!)
            .ToList();

        result.DepartmentCount = groupedDepartments.Count;

        foreach (var departmentGroup in groupedDepartments)
        {
            var departmentName = departmentGroup.Key;

            var extractedTasks = new List<string>();

            foreach (var record in departmentGroup)
            {
                var tasks =
                    _taskExtractionService.ExtractTasks(record.AnaSorumluluk);

                extractedTasks.AddRange(tasks);
            }

            extractedTasks = extractedTasks
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var uniqueTasks = await DeduplicateTasksWithMemoryEmbeddingAsync(
                directorate,
                departmentName,
                extractedTasks);

            var departmentDto = new DepartmentTaskAnalysisDto
            {
                Department = departmentName,
                OriginalTaskCount = extractedTasks.Count,
                UniqueTaskCount = uniqueTasks.Count
            };
            var safeChunkSize = chunkSize <= 0 ? 200 : chunkSize;

            var taskChunks = uniqueTasks
                .Select((task, index) => new { task, index })
                .GroupBy(x => x.index / safeChunkSize)
                .Select(g => g.Select(x => x.task).ToList())
                .ToList();

            foreach (var chunk in taskChunks)
            {
                var prompt = AiPromptBuilder.BuildTaskChunkAnalysisPrompt(
                    directorate,
                    departmentName,
                    chunk);

                var aiResponse = await _aiService.AnalyzeAsync(prompt);

                var parsedTasks = ParseTaskAnalysisItems(aiResponse);

                departmentDto.Tasks.AddRange(parsedTasks);
            }

            result.Departments.Add(departmentDto);
        }

        result.OriginalTaskCount =
            result.Departments.Sum(x => x.OriginalTaskCount);

        result.UniqueTaskCount =
            result.Departments.Sum(x => x.UniqueTaskCount);

        return result;
    }

    private async Task<List<string>> DeduplicateTasksWithMemoryEmbeddingAsync(string directorate, string department, List<string> tasks)
    {
        var memoryIndex = new List<MemoryTaskIndexItemDto>();

        var uniqueTasks = new List<string>();

        foreach (var task in tasks)
        {
            var embedding =
                await _embeddingService.CreateEmbeddingAsync(task);

            var duplicate = false;

            foreach (var indexed in memoryIndex)
            {
                var similarity =
                    _vectorDb.CosineSimilarity(
                        embedding,
                        indexed.Embedding);

                if (similarity >= 0.88)
                {
                    duplicate = true;
                    break;
                }
            }

            if (!duplicate)
            {
                uniqueTasks.Add(task);

                memoryIndex.Add(new MemoryTaskIndexItemDto
                {
                    Directorate = directorate,
                    Department = department,
                    TaskText = task,
                    Embedding = embedding
                });
            }
        }

        return uniqueTasks;
    }

    private List<TaskAiAnalysisItemDto> ParseTaskAnalysisItems(string aiResponse)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(aiResponse))
                return new();

            var cleanJson = aiResponse
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var start = cleanJson.IndexOf('[');
            var end = cleanJson.LastIndexOf(']');

            if (start == -1 || end == -1)
                return new();

            cleanJson =
                cleanJson.Substring(start, end - start + 1);

            var result =
                JsonSerializer.Deserialize<List<TaskAiAnalysisItemDto>>(
                    cleanJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            return result ?? new();
        }
        catch
        {
            return new();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Interfaces;
using TaskAnalysis.Service.Builders;
using Microsoft.Extensions.Caching.Memory;

namespace TaskAnalysis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IResponsiblePersonMatcherService _responsiblePersonMatcherService;
    private readonly ICsvReaderService _csvReaderService;
    private readonly IAnalysisService _analysisService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IRetrievalService _retrieval;
    private readonly IAiService _aiService; 

    //  private readonly IAiService _aiService;

    public AnalysisController(
    ICsvReaderService csvReaderService,
    IAnalysisService analysisService,
    IConfiguration configuration,
    IAiService aiService,
    IMemoryCache cache,
    IResponsiblePersonMatcherService responsiblePersonMatcherService) // IAiMockService aiService
    {
        _csvReaderService = csvReaderService;
        _analysisService = analysisService;
        _configuration = configuration;
        _aiService = aiService;
        _cache = cache;
        _responsiblePersonMatcherService = responsiblePersonMatcherService;
    }

    [HttpGet("raw")]
    public IActionResult GetRawRecords()
    {
        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            return BadRequest("CSV klasör yolu tanımlı değil.");

        var records = _csvReaderService.ReadAllCsv(folderPath);

        return Ok(records);
    }

    [HttpGet("summary")]
    public IActionResult GetSummary()
    {
        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            return BadRequest("CSV klasör yolu tanımlı değil.");

        var records = _csvReaderService.ReadAllCsv(folderPath);
        var summaries = _analysisService.BuildDirectoraterSummaries(records);

        return Ok(summaries);
    }

    [HttpGet("chatbot-context")]
    public IActionResult GetChatbotContext()
    {
        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            return BadRequest("CSV klasör yolu tanımlı değil.");

        var records = _csvReaderService.ReadAllCsv(folderPath);
        var summaries = _analysisService.BuildDirectoraterSummaries(records);
        var chatbotContext = _analysisService.BuildChatbotContext(summaries);

        return Ok(chatbotContext);
    }

    /*    [HttpGet("ai-mock-analysis")]
        public IActionResult GetAiAnalysis()
        {
            var folderPath = _configuration["CsvSettings:FolderPath"];

            if (string.IsNullOrWhiteSpace(folderPath))
                return BadRequest("CSV klasör yolu tanımlı değil.");

            var records = _csvReaderService.ReadAllCsv(folderPath);
            var summaries = _analysisService.BuildDirectoraterSummaries(records);

            if (summaries.Count == 0)
                return BadRequest("Analiz edilecek veri bulunamadı.");

            var prompt = AiPromptBuilder.BuildDirectoratePrompt(summaries[0]);
            var aiResult = _aiService.Analyze(prompt);

            return Ok(new
            {
                Prompt = prompt,
                AiResult = aiResult
            });
        }
    */

    [HttpGet("ai-analysis/{directorate}")]
    public async Task<IActionResult> GetAiAnalysis(string directorate, [FromQuery] string? department)
    {
        var safeDirectorate = directorate?.Trim() ?? string.Empty;
        var safeDepartment = department?.Trim();

        var cacheKey = string.IsNullOrWhiteSpace(safeDepartment)
            ? $"ai-analysis-v3-{safeDirectorate.ToLower()}"
            : $"ai-analysis-v3-{safeDirectorate.ToLower()}-{safeDepartment.ToLower()}";

        if (_cache.TryGetValue(cacheKey, out var cachedResult))
            return Ok(cachedResult);

        try
        {
            var folderPath = _configuration["CsvSettings:FolderPath"];

            if (string.IsNullOrWhiteSpace(folderPath))
                return BadRequest("CSV klasör yolu tanımlı değil.");

            var records = _csvReaderService.ReadAllCsv(folderPath);

            var filtered = records
                .Where(x => !string.IsNullOrWhiteSpace(x.Birim)
                    && x.Birim.Equals(safeDirectorate, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine("Kişi dolu kayıt sayısı: " +
            filtered.Count(x => !string.IsNullOrWhiteSpace(x.ad_soyad)));

            foreach (var item in filtered.Take(5))
            {
                Console.WriteLine($"AD: {item.ad_soyad} | Müdürlük: {item.Mudurluk}");
            }


            if (!string.IsNullOrWhiteSpace(safeDepartment))
            {
                filtered = filtered
                    .Where(x => !string.IsNullOrWhiteSpace(x.Mudurluk)
                        && x.Mudurluk.Equals(safeDepartment, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!filtered.Any())
            {
                return NotFound(string.IsNullOrWhiteSpace(safeDepartment)
                    ? "Bu direktörlük için veri bulunamadı."
                    : "Bu departman için veri bulunamadı.");
            }

            var chunks = filtered
                .Select((record, index) => new { record, index })
                .GroupBy(x => x.index / 20)
                .Select(g => string.Join("\n", g.Select(x =>
                    $"Müdürlük: {x.record.Mudurluk} | " +
                    $"Birim: {x.record.Birim} | " +
                    $"Amaç: {x.record.Amac} | " +
                    $"Yetki: {x.record.Yetki} | " +
                    $"Ana Sorumluluk: {x.record.AnaSorumluluk}"
                )))
                .ToList();

            var partialTasks = chunks.Select(chunk =>
            {
                var chunkPrompt = AiPromptBuilder.BuildDepartmentChunkAnalysisPrompt(
                    chunk,
                    safeDirectorate,
                    safeDepartment
                );

                return _aiService.AnalyzeAsync(chunkPrompt);
            }).ToList();

            var partialAnalyses = (await Task.WhenAll(partialTasks)).ToList();

            var finalPrompt = AiPromptBuilder.BuildFinalDepartmentAnalysisPrompt(
                partialAnalyses,
                safeDirectorate,
                safeDepartment
            );

            var finalAnalysis = await _aiService.AnalyzeAsync(finalPrompt);

            var analyzedTask = _aiService.ParseTaskAnalysis(finalAnalysis);

            analyzedTask.ResponsiblePeople =
                _responsiblePersonMatcherService.FindResponsiblePeople(
                    filtered,
                    $"{analyzedTask.Task} {analyzedTask.ProjectIdea} {analyzedTask.Recommendation} {analyzedTask.BestSolution}",
                    5
                );

            var result = new
            {
                directorate = safeDirectorate,
                department = safeDepartment,
                recordCount = filtered.Count,
                chunkCount = chunks.Count,
                analysis = analyzedTask,
                fromCache = false
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"AI analizi sırasında hata oluştu: {ex.Message}");
        }
    }

    [HttpGet("ai-unique-tasks")]
    public async Task<IActionResult> GetAiUniqueTasks()
    {
        var cacheKey = $"ai-unique-tasks"; // Validation Model olarak düzeltielecek kod tekrarı azaltılacak

        if (_cache.TryGetValue(cacheKey, out var cachedResult))
            return Ok(cachedResult);

        try
        {
            var folderPath = _configuration["CsvSettings:FolderPath"];

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return BadRequest("CSV klasör yolu tanımlı değil.");
            }

            var records = _csvReaderService.ReadAllCsv(folderPath);
            var summaries = _analysisService.BuildDirectoraterSummaries(records);
            var uniqueTasks = _analysisService.BuildUniqueTask(summaries);

            if (uniqueTasks.Count == 0)
            {
                return BadRequest("Analiz edilecek uniq görev bulunamadı.");
            }

            var prompt = AiPromptBuilder.BuildUniqueTasksPrompt(uniqueTasks);
            var aiResult = await _aiService.AnalyzeAsync(prompt);

            _cache.Set(cacheKey, TimeSpan.FromMinutes(15));
            return Ok(new
            {
                Prompt = prompt,
                AiResult = aiResult
            });


        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "AI uniq görev analizi sırasında hata oluştu.");
            return StatusCode(500, "AI uniq görev analizi sırasında beklenmeyen bir hata oluştu.");
        }
    }

    [HttpPost("chatbot-ask")]
    public async Task<IActionResult> Ask([FromBody] ChatbotQuestionDto request)
    {
        try
        {   
        var result = await _analysisService.AskQuestionAsync(request);
        return Ok(result);
        }
        catch (Exception ex)
        {
          return BadRequest(ex.Message);
        }
    }

    [HttpPost("index-csv")]
    public async Task<IActionResult> IndexCsv([FromQuery] string fileName)
    {
        var result = await _retrieval.IndexCsvAsync(fileName);
        return Ok(result);
    }

    [HttpPost("index-all-csv")]
    public async Task<IActionResult> IndexAllCsv()
    {
        var result = await _retrieval.IndexAllCsvAsync();
        return Ok(result);
    }

}


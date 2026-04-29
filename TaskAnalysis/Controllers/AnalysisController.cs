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
    private readonly ICsvReaderService _csvReaderService;
    private readonly IAnalysisService _analysisService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IAiService _aiService;

    //  private readonly IAiService _aiService;

    public AnalysisController(
    ICsvReaderService csvReaderService,
    IAnalysisService analysisService,
    IConfiguration configuration,
    IAiService aiService,
    IMemoryCache cache) // IAiMockService aiService
    {
        _csvReaderService = csvReaderService;
        _analysisService = analysisService;
        _configuration = configuration;
        _aiService = aiService;
        _cache = cache;
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

    [HttpGet("raw-txt")]
    public IActionResult GetRawTxt()
    {
        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            return BadRequest("Text klasör yolu tanımlı değil.");

        // 📌 Artık text okuyan metodu çağırıyoruz
        var records = _csvReaderService.ReadAllTxt(folderPath);

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
    public async Task<IActionResult> GetAiAnalysis(string directorate)
    {
        var cacheKey = $"ai-analysis-{directorate.ToLower()}";

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
                         && x.Birim.Equals(directorate, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!filtered.Any())
                return NotFound("Bu direktörlük için veri bulunamadı.");

            var uniqueTasks = filtered
                .Where(x => !string.IsNullOrWhiteSpace(x.AnaSorumluluk))
                .GroupBy(x => x.AnaSorumluluk.Trim().ToLower())
                .Select(g => new UniqueTaskDto
                {
                    Task = g.First().AnaSorumluluk,
                    Departments = g.Select(x => x.Mudurluk)
                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .OrderBy(x => x)
                                   .ToList()
                })
                .ToList();

            // Normalize
            var normalizePrompt = AiPromptBuilder.BuildNormalizeTasksPrompt(uniqueTasks);
            var normalizeResult = await _aiService.AnalyzeAsync(normalizePrompt);
            var normalizedTasks = _aiService.ParseUniqueTasks(normalizeResult);

            // Analysis
            var analysisPrompt = AiPromptBuilder.BuildUniqueTasksPrompt(normalizedTasks);
            var analysisResult = await _aiService.AnalyzeAsync(analysisPrompt);
            var analyzedTasks = _aiService.ParseUniqueTasks(analysisResult);

            var result = new DirectortateUniqueTaskAnalysisDto
            {
                Directortate = directorate,
                Tasks = analyzedTasks
            };

            // Cache set burada yapılmalı
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));

            return Ok(result);
        }
        catch (Exception ex)
        {
            // Hata loglama eklenebilir
            return StatusCode(500, $"AI analizi sırasında hata oluştu: {ex.Message}");
        }
    }

    [HttpGet("ai-unique-tasks")]
    public async Task<IActionResult> GetAiUniqueTasks()
    {
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
        var result = await _analysisService.AskQuestionAsync(request);
        return Ok(result);
    }

}


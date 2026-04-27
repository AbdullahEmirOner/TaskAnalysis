using Microsoft.AspNetCore.Mvc;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Interfaces;
using TaskAnalysis.Service.Builders;

namespace TaskAnalysis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly ICsvReaderService _csvReaderService;
    private readonly IAnalysisService _analysisService;
    private readonly IConfiguration _configuration;
    private readonly IAiService _aiService;
  //  private readonly IAiService _aiService;

    public AnalysisController(
    ICsvReaderService csvReaderService,
    IAnalysisService analysisService,
    IConfiguration configuration,
    IAiService aiService) // IAiMockService aiService
    {
        _csvReaderService = csvReaderService;
        _analysisService = analysisService;
        _configuration = configuration;
        _aiService = aiService;
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
    public async Task<IActionResult> GetAiAnalysisByDirectorate(string directorate)
    {
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

            var normalizePrompt = AiPromptBuilder.BuildNormalizeTasksPrompt(uniqueTasks);
            var normalizeResult = await _aiService.AnalyzeAsync(normalizePrompt);
            var normalizedTasks = _aiService.ParseUniqueTasks(normalizeResult);

            var analysisPrompt = AiPromptBuilder.BuildUniqueTasksPrompt(normalizedTasks);
            var analysisResult = await _aiService.AnalyzeAsync(analysisPrompt);
            var analyzedTasks = _aiService.ParseUniqueTasks(analysisResult);

            var result = new DirectortateUniqueTaskAnalysisDto
            {
                Directortate = directorate,
                Tasks = analyzedTasks
            };

            return Ok(result);
        }
        catch
        {
            return StatusCode(500, "AI analizi sırasında hata oluştu.");
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
        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            return BadRequest("CSV klasör yolu tanımlı değil.");

        var records = _csvReaderService.ReadAllCsv(folderPath);
        var summaries = _analysisService.BuildDirectoraterSummaries(records);
        var context = _analysisService.BuildChatbotContext(summaries);
        var prompt = AiPromptBuilder.BuildChatbotPrompt(context, request.Question);
        var aiResponse = await _aiService.AnalyzeAsync(prompt);

        var cleaned = aiResponse
        .Replace("```json", "")
        .Replace("```", "")
        .Trim();

        var parsed = _aiService.ParseUniqueTasks(cleaned);

        if (parsed == null || !parsed.Any())
        {
            return Ok(new
            {
                raw = aiResponse
            });
        }

        return Ok(parsed);
    }

}


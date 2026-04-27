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
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Soru boş olamaz.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            return BadRequest("CSV dosyası seçilmelidir.");

        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            return BadRequest("CSV klasör yolu tanımlı değil.");

        var safeFileName = Path.GetFileName(request.FileName);
        var filePath = Path.Combine(folderPath, safeFileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound("Seçilen CSV dosyası bulunamadı.");

        var records = _csvReaderService.ReadCsv(filePath);
        return Ok(new
        {
            fileName = safeFileName,
            filePath = filePath,
            exists = System.IO.File.Exists(filePath),
            recordCount = records.Count,
            firstRecord = records.FirstOrDefault(),
            firstAnaSorumluluk = records.FirstOrDefault()?.AnaSorumluluk
        });

        if (records == null || records.Count == 0)
            return BadRequest("CSV okundu ama kayıt bulunamadı.");

        var summaries = _analysisService.BuildDirectoraterSummaries(records);
        var context = _analysisService.BuildChatbotContext(summaries);

        var prompt = AiPromptBuilder.BuildChatbotPrompt(context, request.Question);

        var aiResponse = await _aiService.AnalyzeAsync(prompt);

        return Ok(new
        {
            fileName = safeFileName,
            question = request.Question,
            answer = aiResponse
        });
    }


    [HttpGet("csv-files")]
    public IActionResult GetCsvFiles()
    {
        var folderPath = _configuration["CsvSettings:FolderPath"];

        if (string.IsNullOrWhiteSpace(folderPath))
            return BadRequest("CSV klasör yolu tanımlı değil.");

        if (!Directory.Exists(folderPath))
            return NotFound("CSV klasörü bulunamadı.");

        var files = Directory.GetFiles(folderPath, "*.csv")
        .Select(Path.GetFileName)
        .ToList();

        return Ok(files);
    }

}


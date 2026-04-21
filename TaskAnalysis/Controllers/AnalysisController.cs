using Microsoft.AspNetCore.Mvc;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly ICsvReaderService _csvReaderService;
    private readonly IAnalysisService _analysisService;
    private readonly IConfiguration _configuration;

    public AnalysisController(
    ICsvReaderService csvReaderService,
    IAnalysisService analysisService,
    IConfiguration configuration)
    {
        _csvReaderService = csvReaderService;
        _analysisService = analysisService;
        _configuration = configuration;
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
}

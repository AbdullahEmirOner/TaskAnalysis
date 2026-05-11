using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.DTOs.AIDTOs;
using TaskAnalysis.Core.DTOs.ChatbotDTOs;
using TaskAnalysis.Core.DTOs.DepartmentDTOs;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Interfaces;
using TaskAnalysis.Core.Interfaces.IDbContext;
using TaskAnalysis.Core.Interfaces.IRAG;
using TaskAnalysis.Service.Builders;

namespace TaskAnalysis.API.Controllers;
//------------------------------------------------- CRUD işlemleri maalesef burada oluyor refactoring yapılmalı (Katmanlar iç içe girmiş durumda) --------------------------------------------------------------
//--------------------------------------------- Direktorlükler için service katmanında yeni fonksiyon yazılmalı !!! ve service katmanı da parçalnmalı ----------------------------------------------------------

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IResponsiblePersonMatcherService _responsiblePersonMatcherService;
    private readonly ICsvReaderService _csvReaderService;
    private readonly IAnalysisService _analysisService;
    private readonly IConfiguration _configuration; 
    private readonly IEmbeddingHelperService _embeddingHelperService;
    private readonly IMemoryCache _cache;
    private readonly IRetrievalService _retrieval;
    private readonly IAiService _aiService;
    private readonly IApplicationDbContext _context;

    public AnalysisController(ICsvReaderService csvReaderService, IAnalysisService analysisService, IConfiguration configuration, 
    IAiService aiService,
    IMemoryCache cache,
    IEmbeddingHelperService embeddingHelperService,
    IRetrievalService retrieval,
    IApplicationDbContext context,
    IResponsiblePersonMatcherService responsiblePersonMatcherService) // IAiMockService aiService
    {
        _csvReaderService = csvReaderService;
        _analysisService = analysisService;
        _configuration = configuration;
        _aiService = aiService;
        _cache = cache;
        _context = context;
        _responsiblePersonMatcherService = responsiblePersonMatcherService;
        _retrieval = retrieval;
        _embeddingHelperService = embeddingHelperService;
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

    [HttpGet("ai-analysis/{directorate}")]
    public async Task<IActionResult> GetAiAnalysis(string directorate, [FromQuery] string? department)
    {
        var safeDirectorate = directorate?.Trim() ?? string.Empty;
        var safeDepartment = department?.Trim();

        // Önce DB kontrolü
        var existingDbResult = await _context.DepartmentAiAnalysisResults
            .FirstOrDefaultAsync(x =>
                x.Directorate == safeDirectorate &&
                x.Department == safeDepartment);

        if (existingDbResult != null)
        {
            var dbResult = new
            {
                directorate = existingDbResult.Directorate,
                department = existingDbResult.Department,
                recordCount = existingDbResult.RecordCount,
                chunkCount = existingDbResult.ChunkCount,
                analysis = new AiTaskAnalysisDto
                {
                    Task = existingDbResult.Task,
                    BestSolution = existingDbResult.BestSolution,
                    AutomationRate = existingDbResult.AutomationRate,
                    Recommendation = existingDbResult.Recommendation,
                    ProjectIdeas = JsonSerializer.Deserialize<List<ProjectIdeaDto>>(
                        existingDbResult.ProjectIdeasJson) ?? new(),
                    ResponsiblePeople = JsonSerializer.Deserialize<List<ResponsiblePersonDto>>(
                        existingDbResult.ResponsiblePeopleJson) ?? new()
                },
                fromCache = true,
                source = "database"
            };

            return Ok(dbResult);
        }

        // Cache kontrolü
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

            // Chunk üretimi
            var chunks = filtered
                .Select((record, index) => new { record, index })
                .GroupBy(x => x.index / 200)
                .Select(g => string.Join("\n", g.Select(x =>
                    $"Müdürlük: {x.record.Mudurluk} | " +
                    $"Birim: {x.record.Birim} | " +
                    $"Amaç: {x.record.Amac} | " +
                    $"Yetki: {x.record.Yetki} | " +
                    $"Ana Sorumluluk: {x.record.AnaSorumluluk}"
                )))
                .ToList();

            // Parça analizleri
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

            // Final analiz
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
                    $"{analyzedTask.Task} {string.Join(" ", analyzedTask.ProjectIdeas.Select(p => p.ProjectIdea))} {analyzedTask.Recommendation} {analyzedTask.BestSolution}",
                    5
                );

            var result = new
            {
                directorate = safeDirectorate,
                department = safeDepartment,
                recordCount = filtered.Count,
                chunkCount = chunks.Count,
                analysis = analyzedTask,
                fromCache = false,
                source = "ai-created-and-saved"
            };

            // DB kaydı
            var entity = new DepartmentAiAnalysisResult
            {
                Directorate = safeDirectorate,
                Department = safeDepartment,
                RecordCount = filtered.Count,
                ChunkCount = chunks.Count,
                Task = analyzedTask.Task,
                BestSolution = analyzedTask.BestSolution,
                AutomationRate = analyzedTask.AutomationRate,
                Recommendation = analyzedTask.Recommendation,
                ProjectIdeasJson = JsonSerializer.Serialize(analyzedTask.ProjectIdeas),
                ResponsiblePeopleJson = JsonSerializer.Serialize(analyzedTask.ResponsiblePeople),
                CreatedAt = DateTime.UtcNow
            };

            _context.DepartmentAiAnalysisResults.Add(entity);
            await _context.SaveChangesAsync();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));

            return Ok(result);
        }
        catch (Exception ex)
        {
           // _logger.LogError(ex, "AI analizi sırasında hata oluştu.");
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

    [HttpGet("person/{sicilNo}/ai-analysis")]
    public async Task<IActionResult> AnalyzePersonBySicilNo(string sicilNo)
        {
            var result = await _analysisService.AnalyzePersonBySicilNoAsync(sicilNo);

            return Ok(result);
        }

    [HttpGet("directorate/{directorate}/tasks/ai-analysis")]
    public async Task<IActionResult> AnalyzeDirectorateTasks(string directorate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directorate))
                return BadRequest("Direktörlük boş olamaz.");

            var safeDirectorate = directorate.Trim();

            // 1) Önce DB kontrol
            var existing = await _context.DirectorateTaskAnalysisResults
                .FirstOrDefaultAsync(x => x.Directorate == safeDirectorate);

            if (existing != null)
            {
                var cachedResult =
                    JsonSerializer.Deserialize<DirectorateTaskAnalysisDto>(
                        existing.ResultJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (cachedResult != null)
                {
                    return Ok(new
                    {
                        fromCache = true,
                        source = "database",
                        createdAt = existing.CreatedAt,
                        data = cachedResult
                    });
                }
            }

            var folderPath = _configuration["CsvSettings:FolderPath"];

            if (string.IsNullOrWhiteSpace(folderPath))
                return BadRequest("CSV klasör yolu tanımlı değil.");

            var records = _csvReaderService.ReadAllCsv(folderPath);

            var filtered = records
                .Where(x => !string.IsNullOrWhiteSpace(x.Birim)
                    && x.Birim.Equals(safeDirectorate, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!filtered.Any())
                return NotFound("Bu direktörlük için veri bulunamadı.");

            // 2) Eski endpointteki gibi chunk üretimi
            var chunks = filtered
                .Select((record, index) => new { record, index })
                .GroupBy(x => x.index / 30)
                .Select(g => string.Join("\n", g.Select(x =>
                    $"Müdürlük: {x.record.Mudurluk} | " +
                    $"Birim: {x.record.Birim} | " +
                    $"Amaç: {x.record.Amac} | " +
                    $"Yetki: {x.record.Yetki} | " +
                    $"Ana Sorumluluk: {x.record.AnaSorumluluk}"
                )))
                .ToList();

            var result = new DirectorateTaskAnalysisDto
            {
                Directorate = safeDirectorate
            };

            // 3) Her chunk’ı AI’a görev bazlı analiz ettir
            foreach (var chunk in chunks)
            {
                var prompt = AiPromptBuilder.BuildTaskChunkAnalysisPrompt(
                    safeDirectorate,
                    "MULTIPLE_DEPARTMENTS",
                    new List<string> { chunk }
                );

                var aiResponse = await _aiService.AnalyzeAsync(prompt);

                var parsedTasks = ParseTaskAnalysisItems(aiResponse);

                foreach (var item in parsedTasks)
                {
                    var departmentName = string.IsNullOrWhiteSpace(item.Department)
                        ? "Bilinmeyen Müdürlük"
                        : item.Department;

                    var departmentDto = result.Departments
                        .FirstOrDefault(x => x.Department == departmentName);

                    if (departmentDto == null)
                    {
                        departmentDto = new Core.DTOs.DepartmentDTOs.DepartmentTaskAnalysisDto
                        {
                            Department = departmentName
                        };

                        result.Departments.Add(departmentDto);
                    }

                    departmentDto.Tasks.Add(item);
                }
            }

            foreach (var department in result.Departments)
            {
                department.OriginalTaskCount = department.Tasks.Count;
                department.UniqueTaskCount = department.Tasks.Count;
            }

            result.DepartmentCount = result.Departments.Count;
            result.OriginalTaskCount = result.Departments.Sum(x => x.OriginalTaskCount);
            result.UniqueTaskCount = result.Departments.Sum(x => x.UniqueTaskCount);

            // 4) DB’ye kaydet
            var entity = new DirectorateTaskAnalysisResult
            {
                Directorate = safeDirectorate,
                ResultJson = JsonSerializer.Serialize(result),
                CreatedAt = DateTime.UtcNow
            };

            _context.DirectorateTaskAnalysisResults.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                fromCache = false,
                source = "ai-created-and-saved",
                createdAt = entity.CreatedAt,
                chunkCount = chunks.Count,
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                $"Görev bazlı AI analizi sırasında hata oluştu: {ex.Message}");
        }
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

            cleanJson = cleanJson.Substring(start, end - start + 1);

            var result = JsonSerializer.Deserialize<List<TaskAiAnalysisItemDto>>(
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
  
    /* [HttpGet("ai-mock-analysis")]
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

} 


using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskAnalysis.Core.DTOs;


namespace TaskAnalysis.Service;

public class AiService //: IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> AnalyzeAsync(string prompt)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        //var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        // powershell --> setx OPENAI_API_KEY " "
        var model = _configuration["OpenAI:Model"] ?? "gpt-4.1-mini";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key tanımlı değil.");

        _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = model,
            input = prompt
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("https://api.openai.com/v1/responses", content);

        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"OpenAI hatası: {response.StatusCode} - {responseText}");

        using var document = JsonDocument.Parse(responseText);

        if (document.RootElement.TryGetProperty("output_text", out var outputText))
            return outputText.GetString() ?? string.Empty;

        return responseText;
    }

    public List<UniqueTaskDto> ParseUniqueTasks(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<UniqueTaskDto>();

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<List<UniqueTaskDto>>(json, options);

            return result ?? new List<UniqueTaskDto>();
        }
        catch
        {
            return new List<UniqueTaskDto>();
        }
    }


    private AiDirectorateDto CreateFallback(string directorateName)
    {
        return new AiDirectorateDto
        {
            Directorate = directorateName,
            Departments = new List<AiDepartmentDto>()
        };
    }
}

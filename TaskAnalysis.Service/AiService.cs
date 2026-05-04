using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    /*
     HttpClient, REST API’lere veya herhangi bir web servisine GET, POST, PUT, DELETE 
     gibi HTTP istekleri göndermeye yarar.
    */
    private readonly IConfiguration _configuration;

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /*OpenAI API’sine 
      * HTTP isteği atıp cevap döndürme*/

    /*  public async Task<string> AnalyzeAsync(string prompt)
      {
          var apiKey = _configuration["OpenAI:ApiKey"];
          //var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
          // powershell --> setx OPENAI_API_KEY " "
          var model = _configuration["OpenAI:Model"] ?? "gpt-5.4-nano";

          if (string.IsNullOrWhiteSpace(apiKey))
              throw new InvalidOperationException("OpenAI API key tanımlı değil.");

          _httpClient.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", apiKey);

          var requestBody = new
          {
              model = model,
              input = prompt
          };

          var json = JsonSerializer.Serialize(requestBody); // C# nesnesini JSON formatına dönüştürmek.
          using var content = new StringContent(json, Encoding.UTF8, "application/json");

          using var response = await _httpClient.PostAsync("https://api.openai.com/v1/responses", content);

          var responseText = await response.Content.ReadAsStringAsync();

          if (!response.IsSuccessStatusCode)
              throw new Exception($"OpenAI hatası: {response.StatusCode} - {responseText}");

          using var document = JsonDocument.Parse(responseText);

          if (document.RootElement.TryGetProperty("output_text", out var outputText))
              return outputText.GetString() ?? string.Empty;

          return responseText;
      }*/

    public async Task<string> AnalyzeAsync(string prompt)
    {
        var endpoint = "https://openai-web-swe.openai.azure.com/"; // string olarak // Azure portalda Keys and Endpoint kısmında gördüğün URL
        var deploymentName = "gpt-5-chat"; // Azure’da oluşturduğun deployment adı
        var apiKey = "b0876dfa28804928a76ea09e8115b5e6"; // Azure portalda aldığın geçerli key
        var apiVersion = "2025-04-01-preview";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("Azure OpenAI API key yok.");

        // Doğru endpoint formatı
        var requestUrl = $"{endpoint}openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

        // Azure’un beklediği body formatı
        var requestBody = new
        {
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
      
        Console.WriteLine("URL: " + requestUrl);
        Console.WriteLine("KEY LENGTH: " + apiKey.Length);
        Console.WriteLine("DEPLOYMENT: " + deploymentName);

        using var response = await _httpClient.PostAsync(requestUrl, content);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Azure OpenAI hatası: {(int)response.StatusCode} {response.StatusCode} - {responseText}");

        using var document = JsonDocument.Parse(responseText);

        // Response parsing
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
    public AiTaskAnalysisDto ParseTaskAnalysis(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return new AiTaskAnalysisDto();

            json = json
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var start = json.IndexOf('{');
            var end = json.LastIndexOf('}');

            if (start == -1 || end == -1)
                return new AiTaskAnalysisDto { Recommendation = json };

            var cleanJson = json.Substring(start, end - start + 1);

            var result = JsonSerializer.Deserialize<AiTaskAnalysisDto>(
                cleanJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null)
                return new AiTaskAnalysisDto { Recommendation = json };

            // AI bazen asıl JSON'u recommendation içine gömüyor, bunu yakalıyoruz
            if (string.IsNullOrWhiteSpace(result.ProjectIdea) &&
                !string.IsNullOrWhiteSpace(result.Recommendation) &&
                result.Recommendation.Contains("{") &&
                result.Recommendation.Contains("projectIdea"))
            {
                var innerStart = result.Recommendation.IndexOf('{');
                var innerEnd = result.Recommendation.LastIndexOf('}');

                if (innerStart != -1 && innerEnd != -1)
                {
                    var innerJson = result.Recommendation.Substring(
                        innerStart,
                        innerEnd - innerStart + 1
                    );

                    var innerResult = JsonSerializer.Deserialize<AiTaskAnalysisDto>(
                        innerJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (innerResult != null)
                        return innerResult;
                }
            }

            return result;
        }
        catch
        {
            return new AiTaskAnalysisDto { Recommendation = json };
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

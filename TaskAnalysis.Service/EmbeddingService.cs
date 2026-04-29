using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskAnalysis.Core.Interfaces;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;

    public EmbeddingService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<float>> CreateEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            model = "text-embedding-3-small",
            input = text
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "OPENAI_API_KEY");

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToList();   // <-- ToList() kullan
    }

}

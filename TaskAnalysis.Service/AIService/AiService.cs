using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.AIService;

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
        var deploymentName = "gpt-5-chat"; // Azure’da oluşturduğum deployment adı
        var apiKey = "b0876dfa28804928a76ea09e8115b5e6"; // Azure portalda aldığım geçerli key
        var apiVersion = "2025-04-01-preview";
        // Yukarıdaki yapı sisteme gömülmesi gerekyior.
        // Bunları genellikle appsettings.json veya environment variable olarak saklamak daha güvenli olurdu.

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("Azure OpenAI API key yok.");

        // Doğru endpoint formatı
        var requestUrl = $"{endpoint}openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("api-key", apiKey); //  Bundan sonra _httpClient ile yaptığın her istekte şu header otomatik olarak bulunur
        /*
         DefaultRequestHeaders.Clear() → Daha önce eklenmiş tüm varsayılan header’ları temizliyorsun. Yani HttpClient üzerinden yapılacak her istekte otomatik giden header’lar sıfırlanıyor.
         DefaultRequestHeaders.Add("api-key", apiKey) → Azure OpenAI gibi servislerin kimlik doğrulaması için gereken api-key header’ını ekliyorsun.
         Burada "api-key" header adı, apiKey ise senin Azure portalından aldığın gizli anahtar (GUID benzeri string).
         */
        // Azure’un beklediği body formatı
        var requestBody = new
        {
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };
        /*
         temperature = 0.2 aslında AI modelinin cevap üretirken ne kadar “yaratıcı” veya “rastgele” davranacağını belirleyen bir parametre.

         Temperature düşük (0.0–0.3) → Model daha deterministik çalışır. Yani aynı soruya hep benzer, güvenilir, tutarlı cevaplar verir. 
         Örneğin 0.2 değeri, modelin daha kontrollü, ciddi ve tahmin edilebilir cevaplar üretmesini sağlar.
         
         Temperature orta (0.5–0.7) → Biraz daha çeşitlilik gelir. Cevaplar hâlâ mantıklı ama farklı alternatifler üretmeye başlar.
         
         Temperature yüksek (0.8–1.0) → Model daha yaratıcı, serbest ve bazen riskli cevaplar verir. Farklı fikirler, alışılmadık cümleler çıkabilir ama tutarlılık azalabilir.
         */
        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

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
    /* 🧩 Fonksiyon Akış Şeması --> Task<string> AnalyzeAsync(string prompt)
    - Konfigürasyon Değerleri Alınıyor

    endpoint → Azure OpenAI URL’si

    deploymentName → Modelin Azure’daki deployment adı

    apiKey → Azure portalından alınan key

    apiVersion → Kullanılacak API sürümü

    API Key Kontrolü

    Eğer apiKey boşsa → Exception fırlatılır: "Azure OpenAI API key yok."

    Request URL Hazırlanıyor

    Format:

    Code
    {endpoint}openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}
    HTTP Header Ayarı

    DefaultRequestHeaders.Clear() → Önceki header’lar temizlenir

    DefaultRequestHeaders.Add("api-key", apiKey) → Kimlik doğrulama için API key eklenir

    Request Body Hazırlanıyor

    messages → Kullanıcı prompt’u (role = "user")

    temperature = 0.2 → Daha deterministik cevap için

    Body JSON’a Çevriliyor

    JsonSerializer.Serialize(requestBody)

    HTTP POST İsteği Gönderiliyor

    PostAsync(requestUrl, content)

    Response Alınıyor

    response.Content.ReadAsStringAsync() → JSON string olarak yanıt

    Başarısızlık Kontrolü

    Eğer response.IsSuccessStatusCode == false → Exception fırlatılır (status code + hata mesajı)

    Yanıt Parse Ediliyor

    JsonDocument.Parse(responseText)

    choices[0].message.content → Modelin ürettiği cevap alınır

    Sonuç Döndürülüyor

    Fonksiyon, modelin cevabını string olarak geri döner
    */
    /* 🔗 Özet Şema (Basitleştirilmiş)
    Code
    [Config Values] 
          ↓
    [API Key Check] → Exception (boşsa)
          ↓
    [Build Request URL]
          ↓
    [Set Headers (api-key)]
          ↓
    [Create Request Body]
          ↓
    [POST Request to Azure]
          ↓
    [Check Response Status]
          ↓
    [Parse JSON Response]
          ↓
    [Return Model Answer]
         */

    public AiTaskAnalysisDto ParseTaskAnalysis(string json) // Parse etmek --> Bir veriyi belirli kurallara göre çözümlemek ve anlamlı parçalara ayırmak.
    {
        /* ParseTaskAnalysis(string json)
          AI’den dönen cevabı AiTaskAnalysisDto nesnesine dönüştürmeye çalışır.
          Eğer JSON formatında değilse veya beklenmedik bir yapıda ise, Recommendation alanına ham metni koyarak geri döner. --> Promt kısmında text olarak alınarak bu sorun çözülür ama 
          orada da parse işlemi yapmak gerekiyor
        */
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

            var cleanJson = json.Substring(start, end - start + 1); // AI bazen cevabın başına/sonuna açıklama eklediği için, gelen string içinde JSON dışında metin olabilir.
            // Bu satır, ham string içinden sadece geçerli JSON parçasını ayıklıyor.

            var result = JsonSerializer.Deserialize<AiTaskAnalysisDto>(
                cleanJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // JSON’daki property isimleri büyük/küçük harf duyarlı olmadan eşleşiyor.
                });
            
            if (result == null)
                return new AiTaskAnalysisDto { Recommendation = json };

            // AI bazen asıl JSON'u recommendation içine gömüyor, bunu yakalıyoruz
            if (!string.IsNullOrWhiteSpace(result.Recommendation) &&
                result.Recommendation.Contains("{") &&
                result.Recommendation.Contains("projectIdea"))
            {
                var innerStart = result.Recommendation.IndexOf('{');
                var innerEnd = result.Recommendation.LastIndexOf('}');

                if (innerStart != -1 && innerEnd != -1)
                {
                    /* Substring(innerStart, innerEnd - innerStart + 1) → Bu iki index arasındaki kısmı alır. 
                     Yani Recommendation içine gömülmüş JSON’un sadece { ... } parçasını çıkarır.
                    */
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

}

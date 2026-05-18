using System.Text.Json;
using TaskAnalysis.Core.DTOs.AIDTOs;
using TaskAnalysis.Core.DTOs.PersonDTOs;
using TaskAnalysis.Core.Interfaces.IAIService;

namespace TaskAnalysis.Service.Helpers
{
    public class ParseHeleprService : IParseHeleprService // Pars fonku bura dışında 2 farklı yerde de var (Controller ve EmbeddingHelperServicede), bir deüzen verilemli
    {
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
        
        public PersonAiAnalysisDto ParsePersonAiAnalysis(string json)
        {
            try
            {
                var cleanJson = json
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                var result = JsonSerializer.Deserialize<PersonAiAnalysisDto>(
                    cleanJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result ?? new PersonAiAnalysisDto();
            }
            catch
            {
                return new PersonAiAnalysisDto
                {
                    GeneralComment = "AI cevabı JSON formatında parse edilemedi."
                };
            }
        }
    }
}

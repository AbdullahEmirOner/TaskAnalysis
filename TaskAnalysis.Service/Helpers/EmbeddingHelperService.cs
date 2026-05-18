using System;
using System.Text.Json;
using TaskAnalysis.Core.DTOs.DepartmentDTOs;
using TaskAnalysis.Core.Interfaces.IRAG;

namespace TaskAnalysis.Service.Helpers
{
    public class EmbeddingHelperService : IEmbeddingHelperService
    {
        public EmbeddingHelperService()
        {
        }

        public int GetStableHash(string value)
        {
            unchecked
            /* unchecked
             C#’ta integer taşması (overflow) normalde hata üretir.

             unchecked → taşma olursa hata verme, sayıyı olduğu gibi devam ettir.
             */
            {
                var hash = 23;

                foreach (var c in value)
                    hash = hash * 31 + c;

                return hash;
            }
            /* Bu kod, string’in her karakterini kullanarak deterministik bir integer hash değeri üretir. 
             * Daha sonra bu değer genellikle indeksleme veya embedding vektörü içinde bir hücre seçmek için kullanılır.
      
             * GetStableHash("a")
                    Başlangıç: 23
                    'a' = 97
                    hash = 23 * 31 + 97 = 804
                    Sonuç: 804
                    
              GetStableHash("ab")
                    Başlangıç: 23
                    'a' = 97 → hash = 804
                    'b' = 98 → hash = 804 * 31 + 98 = 24922
                    Sonuç: 24922
             */
        }

        public void Normalize(float[] vector) // Uzun metinler ve kısa metinleri karşılaştırabilmek yani vektörün uzunluğunu 1 yapmak için var. 
        { /*Normalize işleminin matematiksel dayanağı aslında vektör normu ve cosine similarity(iki vektörün benzerliği ölçmek için ama biz direkt llm'e veriyoruz) kavramlarına dayanıyor. 
        Normalize işlemi → vektörün uzunluğunu 1 yaparak farklı uzunluktaki metinleri karşılaştırmayı kolaylaştırır.
        Vektörün uzunluğu (magnitude) → √(x1² + x2² + ... + xn²)
        Normalize edilmiş vektör → her bileşen / magnitude
        Bu sayede kısa ve uzun metinler arasındaki benzerlik daha adil şekilde ölçülebilir.

        Neden Yapıyoruz?
Uzun metinlerde daha çok kelime olur → vektörde daha çok “1” birikir → vektörün uzunluğu büyür.

Kısa metinlerde daha az kelime olur → vektörün uzunluğu küçük kalır.

Eğer normalize etmezsen, uzun metinler hep daha “büyük” görünür ve benzerlik karşılaştırması bozulur.
        */
            double sum = 0;

            foreach (var value in vector)
                sum += value * value;

            var magnitude = Math.Sqrt(sum);

            if (magnitude == 0)
                return;

            for (int i = 0; i < vector.Length; i++)
                vector[i] = (float)(vector[i] / magnitude);
        }
        /* 🔎 Neden anlam kaybı olmaz?
Normalize → her bileşeni magnitude’a bölüyor.

Bu işlem kelimelerin hangi indekslerde olduğunu değiştirmez, sadece değerlerini küçültür.

Uzun metin → daha çok hücre dolu, kısa metin → daha az hücre dolu. Normalize sonrası da bu fark korunur.

Yani “hangi kelimeler geçti” bilgisi kaybolmaz, sadece vektörün toplam uzunluğu sabitlenir.

📊 Örnek
Uzun metin: "Ali topu Abdullah atarmısın" → 4 kelime → 4 hücre dolu.

Kısa metin: "Ali topu" → 2 kelime → 2 hücre dolu.

Normalize sonrası:

Uzun metin → her hücre değeri ≈ 0.5

Kısa metin → her hücre değeri ≈ 0.707

👉 Hücre sayısı hâlâ farklı → uzun metin daha fazla kelimeyi temsil ediyor.
👉 Normalize sadece “uzunluk farkını” ortadan kaldırıyor, anlamı değil.

📌 Özet
Normalize işlemi anlamı eksiltmez, sadece ölçek farkını kaldırır.

Uzun metinlerdeki kelime çeşitliliği hâlâ vektörde durur.

Bu sayede cosine similarity gibi benzerlik ölçümleri adil çalışır.
         
         */

        public List<TaskAiAnalysisItemDto> ParseTaskAnalysisItems(string aiResponse)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(aiResponse))
                    return new List<TaskAiAnalysisItemDto>();

                var cleanJson = aiResponse
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                var start = cleanJson.IndexOf('[');
                var end = cleanJson.LastIndexOf(']');

                if (start == -1 || end == -1)
                    return new List<TaskAiAnalysisItemDto>();

                cleanJson = cleanJson.Substring(start, end - start + 1);

                var result = JsonSerializer.Deserialize<List<TaskAiAnalysisItemDto>>(cleanJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result ?? new List<TaskAiAnalysisItemDto>();
            }
            catch
            {
                return new List<TaskAiAnalysisItemDto>();
            }
        }
    }
}

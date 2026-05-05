using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.LangChainService;

public class EmbeddingService : IEmbeddingService // Embedding, bir metni veya veriyi sayısal vektörlere dönüştürme işlemidir. Amaç token problemi yaşamadan metinleri karşılaştırmak ve benzerlik ölçmek.
{ 
    private const int VectorSize = 256; // Her metin 256 sayıyla temsil ediliyor.

    public Task<float[]> CreateEmbeddingAsync(string text)
    {
        var vector = new float[VectorSize];

        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult(vector);
        /*string.IsNullOrWhiteSpace(text)
         Bir metot Task<float[]> döndürüyor → bu, asenkron sözleşme demek.

         Eğer sonucu hemen hazır verebiliyorsan (vector zaten hesaplanmışsa), Task.FromResult(vector) kullanıyorsun.
         
         Bu sayede metot imzası (Task<float[]>) korunuyor, ama aslında beklemeye gerek yok → sonuç anında geliyor.

         Çünkü metot imzası Task<float[]> → dışarıdan çağıran kod await ile bekleyebiliyor.

         Eğer burada return vector; yazsaydın, imza bozulurdu (çünkü float[] dönerdi, Task<float[]> değil).
        
         Task.FromResult(vector) → “Bu sonucu zaten hazır, beklemene gerek yok” demek.
         */
        var words = text
            .ToLowerInvariant()
            .Replace(".", " ")
            .Replace(",", " ")
            .Replace(";", " ")
            .Replace(":", " ")
            .Replace("/", " ")
            .Replace("\\", " ")
            .Replace("(", " ")
            .Replace(")", " ")
            .Replace("\"", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        /* Split(' ', StringSplitOptions.RemoveEmptyEntries) → Metni boşluklardan ayırıyor, kelime listesi çıkarıyor.

         RemoveEmptyEntries → arka arkaya gelen boşlukları yok sayıyor.
         Örn: "fatura kontrolü" → ["fatura", "kontrolü"]
         */
        foreach (var word in words)
        {
            var index = Math.Abs(GetStableHash(word)) % VectorSize;
            vector[index] += 1;
        }

        Normalize(vector);

        return Task.FromResult(vector);
    }

    private static int GetStableHash(string value)
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
    }

    private static void Normalize(float[] vector) // Uzun metinler ve kısa metinleri karşılaştırabilmek yani vektörün uzunluğunu 1 yapmak için var. 
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
}
/* AKIŞ
Class her cümle için çalışıyor.

Cümledeki her kelimeyi alıyor.

Her kelimenin her harfini tek tek işleyerek hash üretiyor.

Hash sonucu → 256 uzunluğundaki dizide tek bir indeks.

Aynı kelime her zaman aynı indekse düşüyor → deterministik.

Tüm kelimeler işlendiğinde elimizde 256 boyutlu float[] oluyor.

Normalize işlemiyle bu vektörün uzunluğu 1 yapılıyor → değerler 0–1 aralığına çekiliyor.

Bu vektör artık cümlenin sayısal temsili.

Sen bu vektörü LLM’e veriyorsun → LLM bu sayısal temsili kullanarak yorum yapıyor veya benzerlik karşılaştırması yapıyor.

📌 Yani senin sistemin:
Metin → Kelimeler → Hash → 256 boyutlu vektör → Normalize → LLM’e context olarak gönderme.
 */
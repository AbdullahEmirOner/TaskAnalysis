using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.LangChainService;

public class EmbeddingService : IEmbeddingService // Embedding, bir metni veya veriyi sayısal vektörlere dönüştürme işlemidir.
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

    private static void Normalize(float[] vector)
    {
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

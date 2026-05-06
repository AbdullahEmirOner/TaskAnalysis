using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.LangChainService;

/* Vector DB (vektör veritabanı), yapay zekâ ve makine öğrenimi uygulamalarında kullanılan özel bir veritabanı türüdür. 
   Normal veritabanları satır–sütun yapısında çalışırken, Vector DB verileri yüksek boyutlu matematiksel vektörler olarak saklar ve bu vektörler arasında hızlı benzerlik araması yapar.

📌 Vector DB’nin Temel Özellikleri
Vektör Gömmeleri (Embeddings): Metin, görsel, ses veya video gibi veriler, anlamlarını temsil eden sayısal vektörlere dönüştürülür.

Benzerlik Araması: Bir sorgu vektörüne en yakın diğer vektörleri bulur (ör. “Ali topu attı” ile “Ali topu Abdullah’a attı” benzerliğini yakalar).

Yüksek Boyutlu Veri Yönetimi: Binlerce boyutlu vektörleri depolamak ve sorgulamak için optimize edilmiştir.

Performans: k‑NN (k‑en yakın komşu), HNSW (Hierarchical Navigable Small World) ve IVF (Inverted File Index) gibi algoritmalarla hızlı arama sağlar.

⚙️ Nasıl Çalışır?
Veri Dönüşümü: Makine öğrenmesi modelleri, ham veriyi (ör. bir cümle veya görsel) embedding vektörüne dönüştürür.

Depolama: Bu vektörler veritabanına kaydedilir.

Arama: Kullanıcı bir sorgu yaptığında, sistem vektörler arasındaki mesafeyi (cosine similarity, Öklidyen mesafe) hesaplar.

Sonuç: En yakın vektörler bulunarak benzer içerikler listelenir. 
*/
public class VectorDbService : IVectorDbService
{                    
    private readonly Dictionary<string, List<VectorItemDto>> _store = new(); // _store → dosya adı → embedding listesi şeklinde çalışan bir in‑memory index.

    public Task InsertAsync(string fileName, string text, float[] embedding)
    {
        var safeFileName = Path.GetFileName(fileName);
        
        if (!_store.ContainsKey(safeFileName))
            _store[safeFileName] = new List<VectorItemDto>();

        _store[safeFileName].Add(new VectorItemDto
        {
            Text = text,
            Embedding = embedding
        });

        return Task.CompletedTask;
        /* Bu fonksiyon bir dosya adı altında embedding kayıtlarını saklıyor.

         Her çağrıldığında: "fileName" → "text" + "embedding" eşleşmesini _store içine ekliyor.
         
         _store aslında senin küçük bir vektör veritabanın.
         */
    }

    public Task<List<string>> SearchAsync(string fileName, float[] embedding, int limit = 3)
    // InsertAsync ile embedding kaydediyorsun,
    // SearchAsync ile de “bu embedding’e en yakın cümleleri” buluyorsun. Bu, semantic search pipeline’ının kalbi.
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Task.FromResult(new List<string>());

        var safeFileName = Path.GetFileName(fileName);

        // Case-insensitive eşleştirme
        var key = _store.Keys
            .FirstOrDefault(k => string.Equals(k, safeFileName, StringComparison.OrdinalIgnoreCase));

        if (key == null)
            return Task.FromResult(new List<string>());

        var results = _store[key]
            .Select(x => new
            {
                x.Text,
                Score = CosineSimilarity(x.Embedding, embedding)
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => x.Text)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<List<string>> SearchAllAsync(float[] embedding, int limit = 5) // Tüm dosyalarda kayıtlı embedding’ler arasında arama yapıyor.
    {
        var allItems = _store
            .SelectMany(file => file.Value.Select(item => new
            {
                FileName = file.Key,
                item.Text,
                Score = CosineSimilarity(item.Embedding, embedding)
            }))
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => $"Kaynak CSV: {x.FileName}\n{x.Text}")
            .ToList();

        return Task.FromResult(allItems);
    }

    public bool IsIndexed(string fileName)
    { /* IsIndexed = “Bu dosya için embedding eklenmiş mi?” kontrolü.

       true → dosya adı _store içinde var ve en az bir embedding kayıtlı.
       
       false → hiç eklenmemiş veya liste boş.
       
       👉 Yani bu fonksiyon, senin sisteminde bir dosyanın indexlenip indexlenmediğini anlamak için kullanılıyor.
       */
        var safeFileName = Path.GetFileName(fileName);

        return _store.ContainsKey(safeFileName)
               && _store[safeFileName].Count > 0;
    }

    public void Clear(string fileName) // Çokta gerek yok aslında zaten sabit csv kullanıyoruz tekrar tekrar indexlemeye gerek yok.
    {
        var safeFileName = Path.GetFileName(fileName);

        if (_store.ContainsKey(safeFileName))
            _store.Remove(safeFileName);
    }

    public double CosineSimilarity(float[] v1, float[] v2)
    { /* Bu fonksiyon iki vektör arasındaki cosine similarity değerini döndürüyor.

       Sonuç 0 ile 1 arasında:
       
       1 → tamamen aynı yön (çok benzer)
       
       0 → tamamen farklı yön (benzerlik yok)
       
       Normalize edilmiş vektörlerde bu hesaplama daha doğru çalışıyor.
       */
        var length = Math.Min(v1.Length, v2.Length);

        double dot = 0;
        double mag1 = 0;
        double mag2 = 0;

        for (int i = 0; i < length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-8);
    }

    public int GetTotalItemCount()
    {
        return _store.Sum(x => x.Value.Count);
    }

}

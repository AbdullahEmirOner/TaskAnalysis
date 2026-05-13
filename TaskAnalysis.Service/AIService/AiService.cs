using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TaskAnalysis.Core.Interfaces;

//---------------------------------- BU KOD ARTIK AI ENDPOINTLERİYLE KONUŞMADA BİZE KLAVUZ OLABİLECEK SEVİYEDE BİR ÖRNEK  ----------------------------------//

namespace TaskAnalysis.Service.AIService;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;

    /* HttpClient, REST API’lere veya herhangi bir web servisine GET, POST, PUT, DELETE 
     gibi HTTP istekleri göndermeye yarar.

    Tanım: .NET Framework ve .NET Core’da bulunan bir sınıf. HTTP protokolü üzerinden istek göndermeye ve yanıt almaya yarar.

    Amaç: Programın dış dünyadaki servislerle konuşmasını sağlar. Örneğin bir hava durumu API’sinden veri çekmek veya bir ödeme sistemine bilgi göndermek.
    */

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
    } 
    /*OpenAI API’sine 
      * HTTP isteği atıp cevap döndürme*/

    public async Task<string> AnalyzeAsync(string prompt)
    {
        var endpoint = "https://openai-web-swe.openai.azure.com/"; // string olarak // Azure portalda Keys and Endpoint kısmında gördüğün URL
        var deploymentName = "gpt-5-chat"; // Azure’da oluşturduğum deployment adı
        var apiKey = "b0876dfa28804928a76ea09e8115b5e6"; // Azure portalda aldığım geçerli key
        var apiVersion = "2025-04-01-preview";
        // Yukarıdaki yapı sisteme gömülmesi gerekyior.
        // Bunları genellikle appsettings.json veya environment variable olarak saklamak daha güvenli olurdu.

        /* 🌍 Ortam Değişkenleri Nedir?
            ----------------------------
                Ortam değişkenleri, bilgisayara “önemli bilgileri” saklayan küçük notlardır.  
                
                Bu notlar sayesinde programlar ve işletim sistemi şunu öğrenir:
                
                Dosyaları nerede bulacak,
                
                Geçici dosyaları nereye koyacak,
                
                Hangi ayarlarla çalışacak.
            ----------------------------    
           ⚙️ Ne İşe Yarar?

                Program bulma → PATH sayesinde bilgisayar, python gibi komutların hangi klasörde olduğunu bulur.
                
                Geçici dosya yönetimi → TEMP sayesinde programlar geçici dosyalarını nereye kaydedeceklerini bilir.
                
                Kullanıcı klasörleri → USERPROFILE ile senin masaüstün ve belgelerin bulunur.
                
                Sistem klasörleri → WINDIR ile Windows’un kurulu olduğu klasör öğrenilir.
                
                Güvenlik ve gizlilik → API key gibi gizli bilgileri kodun içine yazmadan ortam değişkeninde saklayabilirsin.
           ------------------------------     
           🎯 Günlük Hayattan Basit Örnek

                Komut satırına python yazarsın → bilgisayar PATH’e bakar, Python’u bulur.
                
                Word geçici dosya oluşturur → TEMP’e bakar, dosyayı doğru klasöre koyar.
                
                Bir uygulama senin masaüstünü bulur → USERPROFILE’e bakar.
                
                Bir yazılım geliştirici API key’i kod yerine ortam değişkenine koyar → güvenli çalışır.
            -----------------------------
           👉 Kısacası: Ortam değişkenleri bilgisayara yol gösteren işaretlerdir.  
                Onlar olmadan programlar “dosyaları nerede bulacağını” bilemez.
         */

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("Azure OpenAI API key yok.");

        // Doğru endpoint formatı
        var requestUrl = $"{endpoint}openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";
        
        _httpClient.DefaultRequestHeaders.Clear(); // Önceden eklenmiş tüm header’ları temizler. Böylece çakışma veya gereksiz header kalmaz.
        _httpClient.DefaultRequestHeaders.Add("api-key", apiKey); //  Bundan sonra _httpClient ile yaptığın her istekte şu header otomatik olarak bulunur
        /* DefaultRequestHeaders.Clear() → Daha önce eklenmiş tüm varsayılan header’ları temizliyorsun. Yani HttpClient üzerinden yapılacak her istekte otomatik giden header’lar sıfırlanıyor.
         DefaultRequestHeaders.Add("api-key", apiKey) → Azure OpenAI gibi servislerin kimlik doğrulaması için gereken api-key header’ını ekliyorsun.
         Burada "api-key" header adı, apiKey ise senin Azure portalından aldığın gizli anahtar (GUID benzeri string).
         */

        var requestBody = new
        { 
        /* Azure’un beklediği body formatı
new → Burada anonim bir nesne oluşturuyorsun.Yani adı olmayan bir sınıf, sadece JSON’a çevrilmek için var.

messages → Bu nesnenin içinde messages adında bir alan var.

new[] { ... } → Bu bir dizi(array) oluşturuyor.

Dizinin içinde yine bir anonim nesne var: { role = "user", content = prompt }.

role = "user" → Mesajın kimden geldiğini söylüyor(user, assistant, system).

content = prompt → Mesajın içeriğini tutuyor.

temperature → Bu da aynı nesnenin başka bir alanı.Modelin cevap üretirken ne kadar “yaratıcı” olacağını belirleyen parametre.

0.2 → Daha deterministik, güvenilir cevaplar.*/
            messages = new[]
        /* new[] { ... } Ne Demek?
Array (dizi) oluşturur.

İçine koyduğun elemanların tipine bakar, dizinin tipini otomatik belirler.

Yani new[] { 1, 2, 3 } → int[] olur.

Senin örneğinde → anonim nesnelerden oluşan bir dizi (AnonimTip[]).
             */
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        /* temperature = 0.2 aslında AI modelinin cevap üretirken ne kadar “yaratıcı” veya “rastgele” davranacağını belirleyen bir parametre.

         Temperature düşük (0.0–0.3) → Model daha deterministik çalışır. Yani aynı soruya hep benzer, güvenilir, tutarlı cevaplar verir. 
         Örneğin 0.2 değeri, modelin daha kontrollü, ciddi ve tahmin edilebilir cevaplar üretmesini sağlar.
         
         Temperature orta (0.5–0.7) → Biraz daha çeşitlilik gelir. Cevaplar hâlâ mantıklı ama farklı alternatifler üretmeye başlar.
         
         Temperature yüksek (0.8–1.0) → Model daha yaratıcı, serbest ve bazen riskli cevaplar verir. Farklı fikirler, alışılmadık cümleler çıkabilir ama tutarlılık azalabilir.
         */

        var json = JsonSerializer.Serialize(requestBody); // Serialize → C# nesnesini JSON string’e çevirir.
        using var content = new StringContent(json, Encoding.UTF8, "application/json"); // content değişkeni, JSON verisini UTF-8 ile kodlanmış ve Content-Type = application/json olan bir HTTP body haline getiriyor.
                                                                                        // StringContent = HTTP isteğinin gövdesini string olarak hazırlayan sınıf.  
        /* Content-Type → HTTP isteğinde veya yanıtında gönderilen verinin türünü belirtir.

        "application/json" → Gönderilen verinin JSON formatında olduğunu sunucuya bildirir.
         */
        
        using var response = await _httpClient.PostAsync(requestUrl, content);
        /* POST isteğini gönderiyor,

         Sunucudan yanıt alıyor,
         
         Yanıtı response nesnesine koyuyor.
         */
        var responseText = await response.Content.ReadAsStringAsync(); // String bir biçimde yanıtın içeriğini okur. Yani JSON string olarak modelin cevabını alır.

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Azure OpenAI hatası: {(int)response.StatusCode} {response.StatusCode} - {responseText}");

        using var document = JsonDocument.Parse(responseText); // JSON string’i tekrar nesneye çevirir (okuma için).

        // Response parsing
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
        /* 📌 Ne Yapıyor?
RootElement → JSON’un en üst seviyesini temsil ediyor.

GetProperty("choices") → Yanıtta choices adında bir alan arıyor.

[0] → İlk elemanı alıyor (genelde modelin ilk cevabı).

GetProperty("message") → GetProperty("content") → Mesajın içeriğine kadar iniyor.

GetString() → İçeriği string olarak alıyor.

?? string.Empty → Eğer içerik null ise boş string döndür.

⚠️ Neden Var?
Çünkü OpenAI Chat API gibi servisler yanıtı şu formatta döndürür:

json
{
  "choices": [
    {
      "message": {
        "role": "assistant",
        "content": "Merhaba Emir!"
      }
    }
  ]
}
Bu zincir tam olarak "Merhaba Emir!" kısmını almak için yazılmıştır. 
         */
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

}

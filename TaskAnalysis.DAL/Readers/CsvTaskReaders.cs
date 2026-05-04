using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.DAL.Readers
{
    public class CsvTaskReaders : ICsvReaderService
    {
        public List<TaskRecord> ReadAllCsv(string folderPath)
        {
            if(string.IsNullOrWhiteSpace(folderPath))
                /*IsNullOrWhiteSpace
                 Nerede Kullanılır?
                    Validasyon: Formdan gelen verilerin gerçekten dolu olup olmadığını kontrol etmek için.
                    
                    Filtreleme: Boş veya sadece boşluklardan oluşan kayıtları ayıklamak için.
                    
                    Temizlik: Kullanıcı girişlerini kontrol ederken gereksiz boşlukları yakalamak için.

               📌 Kısacası: string.IsNullOrWhiteSpace → “Bu string gerçekten dolu mu, yoksa boş/boşluk mu?” sorusuna cevap verir.
                 */
                throw new ArgumentException("Klasör yolu boş olamaz", nameof(folderPath));
            /* nameof
             nameof(folderPath) → "folderPath" döner.
             Yani kodda kullandığın değişkenin ismi string olarak elde edilir.
             */

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException("Klasör bulunamadı: " + folderPath);

            var allRecord = new List<TaskRecord>();
            var files = Directory.GetFiles(folderPath, "*.csv"); // --> using System.IO; burdan geliyor

            foreach (var file in files)
            {
                var sourceFile = Path.GetFileName(file);
                var direktorluk = GetDirektorlukFromFileName(sourceFile);

                using var reader = new StreamReader(file);  //StreamReader, dosyadaki metinleri satır satır veya komple okumanı sağlar.
                // using python'daki wait gibi, dosyayı açar kapar tektek yazmamız gerekmez
                /*StreamReader(file): 
                 Bu, C#’ta bir dosyayı okumak için kullanılan sınıftır. 
                 file değişkeni, okunacak dosyanın yolunu veya dosya akışını temsil eder.   
                 */
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) // CultureInfo.InvariantCulture → “Her zaman aynı, kültürden bağımsız, sabit format kullan” demektir.
                /*var config = new CsvConfiguration(CultureInfo.InvariantCulture)  
                 Burada CsvConfiguration sınıfından bir nesne yaratılıyor. 
                 CultureInfo.InvariantCulture ile kültür bağımsız ayarlar kullanılıyor 
                 (örneğin tarih, sayı formatları kültüre göre değişmesin diye

                 Kültür (Culture) yazılımda, sayıların, tarihlerin, para birimlerinin ve 
                 metinlerin nasıl gösterileceğini belirleyen dil ve bölgesel ayarların bütünüdür. 
                 */
                {
                    Delimiter = ";",
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    PrepareHeaderForMatch = args => args.Header.Trim()
                };
                /*CsvConfiguration
                 Bu yapılandırma, CSV dosyasını okurken hata kontrollerini kapatıyor ve 
                 başlıkları boşluklardan arındırarak eşleştiriyor. 
                 Böylece dosya daha toleranslı şekilde okunuyor
                 */

                using var csvReader = new CsvReader(reader, config);
                /*using: 
                 Buradaki using bir using declaration. 
                 Yani reader nesnesi bu scope (metot veya blok) bittiğinde otomatik olarak dispose edilir. 
                 Dispose demek, dosya bağlantısını kapatmak ve belleği serbest bırakmak. 
                 Normalde reader.Dispose() veya reader.Close() çağırman gerekirken, using var bunu senin yerine yapıyor.

                1. StreamReader(file)
                    Bu satır dosya okuma akışını açıyor.
                    
                    reader → dosyanın içeriğini satır satır veya komple okumaya yarayan bir nesne.
                    
                    using sayesinde dosya işin bitince otomatik kapanıyor (Dispose çağrılıyor).
                    
                    📌 Görevi: Dosyayı açmak ve okumak için bir akış sağlamak.
                    
                2. CsvReader(reader, config)
                    Bu satır CsvHelper kütüphanesinden geliyor.
                    
                    CsvReader → CSV dosyasını satır satır okuyup DTO veya model sınıflarına map eden nesne.
                    
                    Parametreler:
                    
                    reader → az önce açtığın dosya akışı (StreamReader).
                    
                    config → CSV okuma ayarları (Delimiter, HeaderValidated, vs.).
                    
                    using burada da aynı mantıkla çalışıyor: işin bitince CsvReader otomatik Dispose ediliyor.
                    
                    📌 Görevi: StreamReader’dan gelen veriyi CSV formatına göre parse etmek.
                 */
                var rows = csvReader.GetRecords<TaskRecordCsvModel>().ToList(); // prop'larla CSV başlıkları eşleşmeli
                /* csvReader.GetRecords<TaskRecordCsvModel>()  
                 CsvHelper kütüphanesinin metodu. 
                 CSV dosyasındaki satırları okuyor ve her satırı TaskRecordCsvModel tipindeki bir nesneye dönüştürüyor. 
                 Yani CSV’deki kolonlar, TaskRecordCsvModel sınıfındaki property’lere map ediliyor.
                 */
                foreach (var row in rows)
                {
                    var record = new TaskRecord
                    {
                        SicilNo = row.SicilNo?.Trim() ?? string.Empty,
                        ad_soyad = row.ad_soyad?.Trim() ?? string.Empty,
                        Birim = direktorluk,
                        Mudurluk = row.Mudurluk?.Trim() ?? string.Empty,
                        Amac = row.Amac?.Trim() ?? string.Empty,
                        Yetki = row.Yetki?.Trim() ?? string.Empty,
                        AnaSorumluluk = row.AnaSorumluluk?.Trim() ?? string.Empty,
                        SourceFile = sourceFile
                    };

                    if (IsMeaningful(record))
                    {
                        allRecord.Add(record);
                    }
                }
            }
            return allRecord;
        }

        private static bool IsMeaningful(TaskRecord record)
        {
            if (string.IsNullOrWhiteSpace(record.AnaSorumluluk))
            {
                return false;
            }
            return !string.IsNullOrWhiteSpace(record.Mudurluk)
                || !string.IsNullOrWhiteSpace(record.Yetki)
                || !string.IsNullOrWhiteSpace(record.Amac);
        }

        private static string GetDirektorlukFromFileName(string fileName) // --> Dosya adını okunabilir bir direktörlük adı haline getiriyor.
        {
            var name = Path.GetFileNameWithoutExtension(fileName);

            return name
                .Replace("_", " ")
                .Replace("-", " ")
                .Trim();
        }

        public List<TaskRecord> ReadCsv(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Dosya yolu boş olamaz", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV dosyası bulunamadı: " + filePath);

            var allRecord = new List<TaskRecord>();
            var sourceFile = Path.GetFileName(filePath);
            var direktorluk = GetDirektorlukFromFileName(sourceFile);

            using var reader = new StreamReader(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                PrepareHeaderForMatch = args => args.Header.Trim()
            };

            using var csvReader = new CsvReader(reader, config);
            var rows = csvReader.GetRecords<TaskRecordCsvModel>().ToList();

            foreach (var row in rows)
            {
                var record = new TaskRecord
                {
                    SicilNo = row.SicilNo?.Trim() ?? string.Empty,
                    ad_soyad = row.ad_soyad?.Trim() ?? string.Empty,
                    Birim = direktorluk,
                    Mudurluk = row.Mudurluk?.Trim() ?? string.Empty,
                    Amac = row.Amac?.Trim() ?? string.Empty,
                    Yetki = row.Yetki?.Trim() ?? string.Empty,
                    AnaSorumluluk = row.AnaSorumluluk?.Trim() ?? string.Empty,
                    SourceFile = sourceFile
                };

                if (IsMeaningful(record))
                {
                    allRecord.Add(record);
                }
            }

            return allRecord;
        }

    }
}
/*AKIŞ
File (CSV)
   ↓
StreamReader → dosya akışı
   ↓
CsvReader(config) → CSV parser
   ↓
GetRecords<TaskRecordCsvModel>() → DTO listesi
   ↓
foreach → TaskRecord nesneleri oluşturuluyor
   ↓
IsMeaningful → filtreleme
   ↓
allRecord → sonuç listesi 
*/

/* AKIŞ 
 📂 1. File (CSV)
Elinde bir CSV dosyası var (örneğin tasks.csv).

İçinde satırlar ve sütunlar bulunuyor: "SicilNo;Mudurluk;Amac;Yetki;AnaSorumluluk" gibi.

📖 2. StreamReader → Dosya Akışı
csharp
using var reader = new StreamReader(file);
Dosyayı açıyor ve ham metin akışı sağlıyor.

Satır satır veya komple dosya içeriğini okuyabilirsin.

using sayesinde iş bitince dosya otomatik kapanıyor.

📌 Görev: Dosya içeriğini okumak için kapı açmak.

🧩 3. CsvReader(config) → CSV Parser
csharp
using var csvReader = new CsvReader(reader, config);
CsvHelper kütüphanesi devreye giriyor.

reader → dosya akışı, config → CSV ayarları (Delimiter = ";", başlık eşleştirme, hata toleransı).

Görev: Dosya akışını CSV formatına göre parse etmek (yani satırları kolonlara ayırmak).

📌 Görev: Ham metni CSV kurallarına göre anlamlı hale getirmek.

🗂 4. GetRecords<TaskRecordCsvModel>() → DTO Listesi
csharp
var rows = csvReader.GetRecords<TaskRecordCsvModel>().ToList();
CSV’deki her satır TaskRecordCsvModel tipine dönüştürülüyor.

CSV başlıkları → DTO property’leri ile eşleşiyor.

Sonuç: rows artık List<TaskRecordCsvModel> tipinde, yani bellekte hazır bir liste.

📌 Görev: CSV satırlarını C# nesnelerine dönüştürmek.

🔄 5. foreach → TaskRecord Nesneleri
csharp
foreach (var row in rows)
{
    var record = new TaskRecord
    {
        SicilNo = row.SicilNo?.Trim() ?? string.Empty,
        Birim = direktorluk,
        Mudurluk = row.Mudurluk?.Trim() ?? string.Empty,
        Amac = row.Amac?.Trim() ?? string.Empty,
        Yetki = row.Yetki?.Trim() ?? string.Empty,
        AnaSorumluluk = row.AnaSorumluluk?.Trim() ?? string.Empty,
        SourceFile = sourceFile
    };
    ...
}
DTO’dan (TaskRecordCsvModel) domain entity’ye (TaskRecord) dönüşüm yapılıyor.

Trim() ile boşluklar temizleniyor, ?? string.Empty ile null güvenli hale getiriliyor.

Burada iş kurallarına uygun hale getirme işlemi yapılıyor.

📌 Görev: DTO → Domain Entity dönüşümü.

✅ 6. IsMeaningful(record) → Filtreleme
csharp
if (IsMeaningful(record))
{
    allRecord.Add(record);
}
IsMeaningful metodu, kaydın gerçekten dolu/işe yarar olup olmadığını kontrol ediyor.

Örneğin Mudurluk, Amac, Yetki, AnaSorumluluk boşsa kaydı atıyor.

Böylece gereksiz satırlar listeye eklenmiyor.

📌 Görev: Veri temizliği ve filtreleme.

📋 7. allRecord → Sonuç Listesi
Tüm anlamlı kayıtlar allRecord listesine ekleniyor.

Metot sonunda bu liste döndürülüyor.

Artık elinde temizlenmiş, işlenmiş, domain entity listesi var.

📌 Görev: Sonuçları toplamak ve geri döndürmek.

Genel Akış
Code
File (CSV)
   ↓
StreamReader → Dosya akışı
   ↓
CsvReader(config) → CSV parser
   ↓
GetRecords<TaskRecordCsvModel>() → DTO listesi
   ↓
foreach → TaskRecord nesneleri oluşturuluyor
   ↓
IsMeaningful → filtreleme
   ↓
allRecord → sonuç listesi
🔑 Özet:

StreamReader → Dosyayı açar.

CsvReader → CSV formatına göre işler.

GetRecords → DTO’ya map eder.

foreach → DTO’dan domain entity’ye dönüştürür.

IsMeaningful → filtreler.

allRecord → temiz sonuç listesi.
 */
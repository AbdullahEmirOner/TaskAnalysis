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
                throw new ArgumentException("Klasör yolu boş olamaz", nameof(folderPath));

            if(!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException("Klasör bulunamadı: " + folderPath);

            var allRecord = new List<TaskRecord>();
            var files = Directory.GetFiles(folderPath, "*.csv");

            foreach (var file in files)
            {
                var sourceFile = Path.GetFileName(file);
                var direktorluk = GetDirektorlukFromFileName(sourceFile);

                using var reader = new StreamReader(file);
                /*StreamReader(file): 
                 Bu, C#’ta bir dosyayı okumak için kullanılan sınıftır. 
                 file değişkeni, okunacak dosyanın yolunu veya dosya akışını temsil eder. 
                 StreamReader, dosyadaki metinleri satır satır veya komple okumanı sağlar.
                 */
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                /*
                 var config = new CsvConfiguration(CultureInfo.InvariantCulture)  
                 Burada CsvConfiguration sınıfından bir nesne yaratılıyor. 
                 CultureInfo.InvariantCulture ile kültür bağımsız ayarlar kullanılıyor 
                 (örneğin tarih, sayı formatları kültüre göre değişmesin diye
                 */
                {
                    Delimiter = ";",
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    PrepareHeaderForMatch = args => args.Header.Trim()
                };
                /*
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
                 */
                var rows = csvReader.GetRecords<TaskRecordCsvModel>().ToList();
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
    }
}

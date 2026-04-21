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
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
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
            return !string.IsNullOrWhiteSpace(record.Mudurluk)
                || !string.IsNullOrWhiteSpace(record.Yetki)
                || !string.IsNullOrWhiteSpace(record.Amac)
                || !string.IsNullOrWhiteSpace(record.AnaSorumluluk);
        }

        private static string GetDirektorlukFromFileName(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);

            return name
                .Replace("_", " ")
                .Replace("-", " ")
                .Trim();
        }
    }
}

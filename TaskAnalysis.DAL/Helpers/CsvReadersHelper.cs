using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using TaskAnalysis.Core.Entities.CSVEntities;
using TaskAnalysis.Core.Interfaces.ICsvReader;

namespace TaskAnalysis.DAL.Helpers
{
    public class CsvReadersHelper : ICsvReadersHelper
    {
        public bool IsMeaningful(TaskRecord record)
        {
            if (string.IsNullOrWhiteSpace(record.AnaSorumluluk))
            {
                return false;
            }
            return !string.IsNullOrWhiteSpace(record.Mudurluk)
                || !string.IsNullOrWhiteSpace(record.Yetki)
                || !string.IsNullOrWhiteSpace(record.Amac);
        }

        public string GetDirektorlukFromFileName(string fileName) // --> Dosya adını okunabilir bir direktörlük adı haline getiriyor.
        {
            var name = Path.GetFileNameWithoutExtension(fileName);

            return name
                .Replace("_", " ")
                .Replace("-", " ")
                .Trim();
        }
    }
}

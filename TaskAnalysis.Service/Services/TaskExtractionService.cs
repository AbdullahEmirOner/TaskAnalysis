using System.Text.RegularExpressions;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.Services // BU sınıfın amacı birebir aynı görevleri tekilleştirmek; ama asıl işi metin içinden görevleri ayıklamak
{
    public class TaskExtractionService : ITaskExtractionService
    {
        public List<string> ExtractTasks(string? text)
        {            /* Bu fonksiyonun amacı:

Metni temizlemek → satır sonları, madde işaretleri, tireler normalize edilir.

Regex ile parçalamak → cümleleri veya numaralı görevleri ayırır.

25+ karakterlik parçaları görev kabul eder.

Normalize edip tekilleştirir.

Eğer hiç görev çıkmazsa, tüm metni tek görev olarak döner.

Sonuç: List<string> içinde benzersiz görev cümleleri.        
*/
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var cleanText = text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("•", ".")
                .Replace("-", ".")
                .Trim();

            var parts = Regex.Split(
                cleanText,
                @"(?<=[.;])|\s(?=\d+\))|\s(?=\d+\.)",
                RegexOptions.IgnoreCase);

            var tasks = parts
                .Select(x => x.Trim())
                .Where(x => x.Length >= 25)
                .Select(NormalizeTask)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (tasks.Count == 0 && cleanText.Length >= 25)
                tasks.Add(NormalizeTask(cleanText));

            return tasks;
        }

        private static string NormalizeTask(string text)
        {
            return Regex.Replace(text, @"\s+", " ").Trim(); //  “Regular Expression” yani düzenli ifade demektir
        }
    }
}
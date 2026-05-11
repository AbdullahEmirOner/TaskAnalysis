using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Entities.CSVEntities
{
    public class TaskRecordCsvModel // Bu ve TaskRecord sınıflarının yapısı aynı, ancak TaskRecordCsvModel sadece CSV'den veri okumak için kullanılır,
    // TaskRecord ise uygulama içinde kullanılacak genel bir modeldir. --> DTOs tarafında da olabilirdi burda da olur
    {
        public string? SicilNo { get; set; } = string.Empty;
    //  public string? Birim { get; set; } = string.Empty;
        public string? Mudurluk { get; set; } = string.Empty;
        public string? Amac { get; set; } = string.Empty;
        public string? Yetki { get; set; } = string.Empty;
        public string? AnaSorumluluk { get; set; } = string.Empty;
        public string ad_soyad { get; set; } = string.Empty;

    }
}

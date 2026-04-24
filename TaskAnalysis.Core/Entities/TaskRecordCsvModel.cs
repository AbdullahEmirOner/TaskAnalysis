using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Entities
{
    public class TaskRecordCsvModel
    {
        public string? SicilNo { get; set; } = string.Empty;
    //    public string? Birim { get; set; } = string.Empty;
        public string? Mudurluk { get; set; } = string.Empty;
        public string? Amac { get; set; } = string.Empty;
        public string? Yetki { get; set; } = string.Empty;
        public string? AnaSorumluluk { get; set; } = string.Empty;
    }
}

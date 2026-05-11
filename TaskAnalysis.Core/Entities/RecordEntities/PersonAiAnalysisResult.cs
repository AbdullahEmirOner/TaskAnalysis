using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Entities
{
    public class PersonAiAnalysisResult
    {
        public int Id { get; set; }

        public string SicilNo { get; set; } = string.Empty;

        public string ResultJson { get; set; } = "{}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

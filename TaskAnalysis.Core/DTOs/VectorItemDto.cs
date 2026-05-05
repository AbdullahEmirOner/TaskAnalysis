using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs
{
    public class VectorItemDto
    {
        public string Text { get; set; } = string.Empty;

        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}

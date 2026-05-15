using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.DTOs.RAGDTOs
{
    public class VectorItemDto // VectorItemDto = metnin AI tarafından aranabilir hale getirilmiş hali.
    {
        public string FileName { get; set; } = string.Empty;
        public string SicilNo { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
/* EmbeddingService
→ VectorItemDto üretir

RetrievalService
→ VectorItemDto kullanır
 */
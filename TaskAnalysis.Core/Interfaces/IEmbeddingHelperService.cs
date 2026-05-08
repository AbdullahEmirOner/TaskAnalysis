using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.DTOs;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IEmbeddingHelperService
    {
       public int GetStableHash(string value);
       public void Normalize(float[] vector);
        public List<TaskAiAnalysisItemDto> ParseTaskAnalysisItems(string aiResponse);
    }
}

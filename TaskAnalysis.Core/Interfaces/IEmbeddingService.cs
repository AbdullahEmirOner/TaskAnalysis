using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IEmbeddingService
    {
        Task<List<float>> CreateEmbeddingAsync(string text);
    }
}

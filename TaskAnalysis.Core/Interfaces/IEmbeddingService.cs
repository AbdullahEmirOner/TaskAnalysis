using System.Threading.Tasks;

namespace TaskAnalysis.Core.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> CreateEmbeddingAsync(string text);
}


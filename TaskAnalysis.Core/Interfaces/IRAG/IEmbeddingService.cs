using System.Threading.Tasks;

namespace TaskAnalysis.Core.Interfaces.IRAG;

public interface IEmbeddingService
{
    Task<float[]> CreateEmbeddingAsync(string text);
}


using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Interfaces;

public interface IVectorDbService
{
    Task InsertAsync(string fileName, string text, float[] embedding);

    Task<List<string>> SearchAsync(string fileName, float[] embedding, int limit = 3);

    bool IsIndexed(string fileName);

    Task<List<string>> SearchAllAsync(float[] embedding, int limit = 5);
    void Clear(string fileName);
    public double CosineSimilarity(float[] v1, float[] v2);
}


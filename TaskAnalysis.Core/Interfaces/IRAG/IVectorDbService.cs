using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Interfaces.IRAG;

public interface IVectorDbService
{
    Task InsertAsync(string fileName, string text, float[] embedding);

    Task<List<string>> SearchAsync(string fileName, float[] embedding, int limit = 3);

    bool IsIndexed(string fileName);

    Task<List<string>> SearchAllAsync(float[] embedding, int limit = 5);
    void Clear(string fileName);
    public double CosineSimilarity(float[] v1, float[] v2);

    Task<List<string>> SearchByPersonAsync(string fileName, string personName, float[] embedding, int limit = 3);
}


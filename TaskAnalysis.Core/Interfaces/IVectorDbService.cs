using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Interfaces;

public interface IVectorDbService
{
    Task InsertAsync(string fileName, string text, float[] embedding);

    Task<List<string>> SearchAsync(string fileName, float[] embedding, int limit = 3);

    bool IsIndexed(string fileName);

    void Clear(string fileName);
}


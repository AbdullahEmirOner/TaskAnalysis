using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IVectorDbService
    {
        Task<List<string>> SearchAsync(float[] embedding);
        Task InsertAsync(string text, float[] embedding);
        Task InitAsync();
    }
}

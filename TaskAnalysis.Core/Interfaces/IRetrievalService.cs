using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.Entities;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IRetrievalService
    {
        public Task<object> IndexAllCsvAsync();
        bool IsValidText(string? value);
        public Task<object> IndexCsvAsync(string fileName);
        public List<string> CreateChunks(List<TaskRecord> records, int chunkSize = 20);
        public Task<List<string>> RetrieveRelevantChunks(string fileName, string question);

    }
}

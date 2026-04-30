using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Entities;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IAnalysisService
    {
        List<DirectorateSummaryDto> BuildDirectoraterSummaries(List<TaskRecord> records);
        string BuildChatbotContext(List<DirectorateSummaryDto> summeries);
        List<UniqueTaskDto> BuildUniqueTask(List<DirectorateSummaryDto> summaries);
        List<TaskRecord> GetRelevantRecords(List<TaskRecord> records, string question, int maxCount = 50);
        Task<string> AskQuestionAsync(ChatbotQuestionDto request);
        Task<object> IndexCsvAsync(string fileName);
        Task<object> IndexAllCsvAsync();
    }
}

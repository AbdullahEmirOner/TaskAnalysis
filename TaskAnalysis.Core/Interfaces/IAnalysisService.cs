using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.DTOs.ChatbotDTOs;
using TaskAnalysis.Core.DTOs.DirectorateDTOs;
using TaskAnalysis.Core.DTOs.PersonDTOs;
using TaskAnalysis.Core.Entities;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IAnalysisService
    {
        List<DirectorateSummaryDto> BuildDirectoraterSummaries(List<TaskRecord> records);

        string BuildChatbotContext(List<DirectorateSummaryDto> summeries);

        List<UniqueTaskDto> BuildUniqueTask(List<DirectorateSummaryDto> summaries);

       // List<TaskRecord> GetRelevantRecords(List<TaskRecord> records, string question, int maxCount = 50);
        Task<string> AskQuestionAsync(ChatbotQuestionDto request);

        Task<PersonAiAnalysisDto> AnalyzePersonBySicilNoAsync(string sicilNo);

        Task<DirectorateTaskAnalysisDto> AnalyzeDirectorateTasksWithMemoryIndexAsync(
            string directorate,
            int chunkSize = 200);
    }
}

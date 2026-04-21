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
        ChatbotContextDto BuildChatbotContext(List<DirectorateSummaryDto> summeries);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.DTOs;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IAiMockService
    {
        //Task<AiDirectorateAnalysisResultDto> AnalyzeDirectorateAsync(AiAnalysisRequestDto request);
        //string Analyze(string prompt); ---> Mock Ai Service
        //List<AiDepartmentDto> ParseAiResponse(string json);
        Task<string> AnalyzeAsync(string prompt);
        AiDirectorateDto ParseAiResponse(string json);
    }
}

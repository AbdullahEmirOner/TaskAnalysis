using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.DTOs.AIDTOs;
using TaskAnalysis.Core.DTOs.PersonDTOs;

namespace TaskAnalysis.Core.Interfaces
{
    public interface IAiService
    {
        //Task<AiDirectorateAnalysisResultDto> AnalyzeDirectorateAsync(AiAnalysisRequestDto request);
        //string Analyze(string prompt); ---> Mock Ai Service
        //List<AiDepartmentDto> ParseAiResponse(string json);

        Task<string> AnalyzeAsync(string prompt);

        public AiTaskAnalysisDto ParseTaskAnalysis(string json);

        PersonAiAnalysisDto ParsePersonAiAnalysis(string json);
    }
}

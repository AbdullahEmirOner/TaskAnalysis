using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.DTOs.AIDTOs;
using TaskAnalysis.Core.DTOs.PersonDTOs;

namespace TaskAnalysis.Core.Interfaces.IAIService
{
    public interface IParseHeleprService 
    {
        public AiTaskAnalysisDto ParseTaskAnalysis(string json);

        public PersonAiAnalysisDto ParsePersonAiAnalysis(string json);
    }
}

using System.Text.Json;
using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service;

// --------------------------------------------------------------------------------------- Mock Service -----------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
/*public class AiMockService //: IAiMockService
{
    public Task<string> AnalyzeAsync(string prompt)
    {
        var analysis = new AiDirectorateDto
        {
            Directorate = "HR",
            Departments = new List<AiDepartmentDto>
            {
                new AiDepartmentDto
                {
                    Department = "Personnel",
                    Analyses = new List<AiTaskAnalysisDto>
                    {
                        new AiTaskAnalysisDto
                        {
                            Task = "Candidate pre-screening and CV evaluation",
                            AiSuitability = "Yes",
                            AutomationRate = 75,
                            EstimatedWeeklyHours = 12,
                            Recommendation = "CV ön eleme ve aday değerlendirme sistemi önerilir."
                        },
                        new AiTaskAnalysisDto
                        {
                            Task = "Interview scheduling",
                            AiSuitability = "Partial",
                            AutomationRate = 55,
                            EstimatedWeeklyHours = 6,
                            Recommendation = "Mülakat planlama destek asistanı önerilir."
                        }
                    }
                },
                new AiDepartmentDto
                {
                    Department = "Training",
                    Analyses = new List<AiTaskAnalysisDto>
                    {
                        new AiTaskAnalysisDto
                        {
                            Task = "Training planning",
                            AiSuitability = "Partial",
                            AutomationRate = 50,
                            EstimatedWeeklyHours = 8,
                            Recommendation = "Eğitim planlama destek asistanı önerilir."
                        }
                    }
                },
                new AiDepartmentDto
                {
                    Department = "Accounting",
                    Analyses = new List<AiTaskAnalysisDto>
                    {
                        new AiTaskAnalysisDto
                        {
                            Task = "Invoice and record classification",
                            AiSuitability = "Yes",
                            AutomationRate = 65,
                            EstimatedWeeklyHours = 10,
                            Recommendation = "Fatura ve kayıt sınıflandırma sistemi önerilir."
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(analysis, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Task.FromResult(json);
    }

    public List<AiDepartmentDto> ParseAiResponse(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<AiDirectorateDto>(json, options);

        return result?.Departments ?? new List<AiDepartmentDto>();
    }

    public List<UniqueTaskDto> ParseUniqueTasks(string json)
    {
        throw new NotImplementedException();
    }
}*/

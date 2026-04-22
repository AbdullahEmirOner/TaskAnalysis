using System.Text.Json;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service;

public class AiService : IAiService
{
    public string Analyze(string prompt)
    {
        // Örnek veri --> ileride prompt'a göre dinamik üretilecek 
        var analysis = new DirectorateAnalysisDto
        {
            DirektorlukAnalizi = "Some tasks in this directorate can be supported by AI.",
            Mudurlukler = new List<DepartmentAnalysisDto>
            {
                new DepartmentAnalysisDto
                {
                    Mudurluk = "Personnel",
                    AiUygunluk = "Yes",
                    OtomasyonOrani = 75,
                    TahminiSaatHaftalik = 12,
                    Oneri = "Candidate pre-screening and CV evaluation system"
                },
                new DepartmentAnalysisDto
                {
                    Mudurluk = "Training",
                    AiUygunluk = "Partial",
                    OtomasyonOrani = 50,
                    TahminiSaatHaftalik = 8,
                    Oneri = "Training planning support assistant"
                }
            }
        };

        // JSON olarak serialize
        return JsonSerializer.Serialize(analysis, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}

// DTO'lar
public class DirectorateAnalysisDto
{
    public string DirektorlukAnalizi { get; set; }
    public List<DepartmentAnalysisDto> Mudurlukler { get; set; }
}

public class DepartmentAnalysisDto
{
    public string Mudurluk { get; set; }
    public string AiUygunluk { get; set; }
    public int OtomasyonOrani { get; set; }
    public int TahminiSaatHaftalik { get; set; }
    public string Oneri { get; set; }
}

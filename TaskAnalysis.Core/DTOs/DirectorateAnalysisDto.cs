namespace TaskAnalysis.Service;

// DTO'lar
public class DirectorateAnalysisDto
{
    public string DirektorlukAnalizi { get; set; }
    public List<DepartmentAnalysisDto> Mudurlukler { get; set; }
}

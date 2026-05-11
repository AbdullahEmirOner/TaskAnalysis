using TaskAnalysis.Core.DTOs.AIDTOs;
using TaskAnalysis.Core.Entities.CSVEntities;

public interface IResponsiblePersonMatcherService
{
    List<ResponsiblePersonDto> FindResponsiblePeople(
        List<TaskRecord> records,
        string text,
        int take = 5
    );
}

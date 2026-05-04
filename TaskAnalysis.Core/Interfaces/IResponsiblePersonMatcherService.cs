using TaskAnalysis.Core.DTOs;
using TaskAnalysis.Core.Entities;

public interface IResponsiblePersonMatcherService
{
    List<ResponsiblePersonDto> FindResponsiblePeople(
        List<TaskRecord> records,
        string text,
        int take = 5
    );
}

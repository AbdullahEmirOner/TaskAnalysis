namespace TaskAnalysis.Core.Interfaces
{
    public interface ITaskExtractionService
    {
        List<string> ExtractTasks(string? text);
    }
}
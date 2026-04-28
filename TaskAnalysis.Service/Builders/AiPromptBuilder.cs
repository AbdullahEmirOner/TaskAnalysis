using System.Text;
using TaskAnalysis.Core.DTOs;

namespace TaskAnalysis.Service.Builders;

public static class AiPromptBuilder
{
    public static string BuildNormalizeTasksPrompt(List<UniqueTaskDto> tasks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a data normalization expert.");
        sb.AppendLine("Your role is to analyze and group similar business tasks.");
        sb.AppendLine("Tasks may contain typos, Turkish character differences, or different wording.");
        sb.AppendLine("Merge tasks that have the same meaning into a single normalized task.");
        sb.AppendLine();
        sb.AppendLine("Output requirements:");
        sb.AppendLine("- Return ONLY valid JSON.");
        sb.AppendLine("- Use English JSON keys exactly as specified.");
        sb.AppendLine("- Write all task names in Turkish.");
        sb.AppendLine();
        sb.AppendLine("Use this exact JSON format:");
        sb.AppendLine("""
       [
         {
           "task": "normalized task name",
           "departments": ["department1", "department2"]
         }
       ]
       """);
        sb.AppendLine();
        sb.AppendLine("Tasks to analyze:");

        foreach (var task in tasks)
        {
            sb.AppendLine($"- Task: {task.Task}");
            sb.AppendLine($"  Departments: {string.Join(", ", task.Departments)}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string BuildUniqueTasksPrompt(List<UniqueTaskDto> tasks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an automation consultant.");
        sb.AppendLine("Analyze the following unique business tasks.");
        sb.AppendLine("For each task, propose a concrete automation project idea.");
        sb.AppendLine("For each task, determine the best solution type.");
       // sb.AppendLine("Possible solution types: AI, RPA, Hybrid, Other.");
        sb.AppendLine("If none of these fit well, propose a new solution type and explain it.");
        sb.AppendLine("Estimate the automation rate (%).");
        sb.AppendLine("Write recommendation text values in Turkish.");
        sb.AppendLine("If there is a similar real-world project already implemented, include its name and a reference link.");
        sb.AppendLine("If no similar project exists, explicitly write 'No similar project found' for both name and link.");
        sb.AppendLine("Return ONLY valid JSON.");
        sb.AppendLine("Use English JSON keys exactly as specified below.");
        sb.AppendLine();
        sb.AppendLine("Use this exact JSON structure:");
        sb.AppendLine("""
    [
      {
        "task": "string",
        "departments": ["string"],
        "bestSolution": "string",
        "automationRate": 0,
        "recommendation": "string",
        "projectIdea": "string",
        "similarProjectName": "string",
        "similarProjectLink": "string"
      }
    ]
    """);
        sb.AppendLine();

        sb.AppendLine("Tasks:");
        foreach (var task in tasks)
        {
            sb.AppendLine($"Task: {task.Task}");
            sb.AppendLine($"Departments: {string.Join(", ", task.Departments)}");
            sb.AppendLine();
        }

        return sb.ToString();
    }


    public static string BuildChatbotPrompt(string context, string question)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a corporate task analysis assistant.");
        sb.AppendLine("Your role is to answer the user's question using ONLY the provided company task data.");
        sb.AppendLine("Strict rules:");
        sb.AppendLine("- Do not use external knowledge.");
        sb.AppendLine("Provide your answer below. If partial information exists, explain what can be inferred and what is missing.");
        sb.AppendLine("- Write the entire answer in Turkish.");
        sb.AppendLine("- Be clear, concise, and professional.");
        sb.AppendLine();
        sb.AppendLine("Company task data:");
        sb.AppendLine(context);
        sb.AppendLine();
        sb.AppendLine("User question:");
        sb.AppendLine(question);
        sb.AppendLine();
        sb.AppendLine("Answer:");

        return sb.ToString();
    }

}

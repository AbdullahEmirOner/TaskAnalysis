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
        sb.AppendLine("Merge ONLY tasks that clearly have the same meaning.");
        sb.AppendLine("Do NOT merge tasks that are even slightly different.");
        sb.AppendLine("If unsure, keep tasks separate.");

        sb.AppendLine();
        sb.AppendLine("Output requirements:");
        sb.AppendLine("- Return ONLY valid JSON.");
        sb.AppendLine("- Use English JSON keys exactly as specified.");
        sb.AppendLine("- Write all task names in Turkish.");

        sb.AppendLine();
        sb.AppendLine("Use this exact JSON format:");
        sb.AppendLine(@"
[
{
""task"": ""normalized task name"",
""departments"": [""department1"", ""department2""]
}
]");

        sb.AppendLine();
        sb.AppendLine("Tasks to analyze:");

        foreach (var task in tasks)
        {
            sb.AppendLine($"- Task: {task.Task}");
            sb.AppendLine($" Departments: {string.Join(", ", task.Departments)}");
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
        sb.AppendLine("Determine the best solution type (AI, RPA, Hybrid, or Other).");

        sb.AppendLine("If none of these fit well, propose a new solution type and explain it.");
        sb.AppendLine("Estimate the automation rate (%) only if confident.");
        sb.AppendLine("Write recommendation text in Turkish.");
        sb.AppendLine("Try to suggest a realistic project based on the task.");
        sb.AppendLine("Only say 'No similar project found' if absolutely necessary.");

        sb.AppendLine();
        sb.AppendLine("Return ONLY valid JSON.");
        sb.AppendLine("Use English JSON keys exactly as specified.");

        sb.AppendLine();
        sb.AppendLine("Use this exact JSON structure:");
        sb.AppendLine(@"
[
{
""task"": ""string"",
""departments"": [""string""],
""bestSolution"": ""string"",
""automationRate"": 0,
""recommendation"": ""string"",
""projectIdea"": ""string"",
""similarProjectName"": ""string"",
""similarProjectLink"": ""string""
}
]");

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

        sb.AppendLine();
        sb.AppendLine("Strict rules:");
        sb.AppendLine("- Do NOT use external knowledge.");
        sb.AppendLine("- Use ONLY the given data.");
        sb.AppendLine("- If exact answer is not directly found:");
        sb.AppendLine(" • Try to infer from related tasks.");
        sb.AppendLine(" • Combine multiple records if needed.");
        sb.AppendLine(" • Explain your reasoning clearly.");
        sb.AppendLine("- Only say 'Verilen veriler bu soruyu yanıtlamak için yeterli değil' if absolutely NO relevant information exists.");
        sb.AppendLine("- Write the entire answer in Turkish.");
        sb.AppendLine("- Be clear, concise, and professional.");

        sb.AppendLine();
        sb.AppendLine("Company task data (structured):");
        sb.AppendLine("Each line is a separate task record.");
        sb.AppendLine(context);

        sb.AppendLine();
        sb.AppendLine("User question:");
        sb.AppendLine(question);

        sb.AppendLine();
        sb.AppendLine("Answer:");

        return sb.ToString();
    }

    public static string BuildDepartmentChunkAnalysisPrompt(string context, string directorate, string? department)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an AI assistant specializing in corporate task analysis.");
        sb.AppendLine("Analyze the following task records.");
        sb.AppendLine("Base your analysis ONLY on the given records.");
        sb.AppendLine("Answer in Turkish.");
        sb.AppendLine();

        sb.AppendLine($"Directorate: {directorate}");

        if (!string.IsNullOrWhiteSpace(department))
            sb.AppendLine($"Department: {department}");

        sb.AppendLine();
        sb.AppendLine("For this chunk, extract the following:");
        sb.AppendLine("1. Main task themes");
        sb.AppendLine("2. Repeated responsibilities");
        sb.AppendLine("3. Tasks that could be improved with automation or AI");
        sb.AppendLine("4. Possible project ideas");
        sb.AppendLine();
        sb.AppendLine("Task records:");
        sb.AppendLine(context);

        return sb.ToString();
    }
    public static string BuildFinalDepartmentAnalysisPrompt(List<string> partialAnalyses, string directorate, string? department)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an AI assistant performing high-level corporate analysis.");
        sb.AppendLine("Below are partial analyses generated for the same directorate/department.");
        sb.AppendLine("Merge them into a single, consistent, and professional final report.");
        sb.AppendLine("Combine repeated items.");
        sb.AppendLine("Answer in Turkish.");
        sb.AppendLine();

        sb.AppendLine($"Directorate: {directorate}");

        if (!string.IsNullOrWhiteSpace(department))
            sb.AppendLine($"Department: {department}");

        sb.AppendLine();
        sb.AppendLine("The final report should include the following sections:");
        sb.AppendLine("1. General Summary");
        sb.AppendLine("2. Main Task Areas");
        sb.AppendLine("3. Repeated Tasks");
        sb.AppendLine("4. AI / Automation Opportunities");
        sb.AppendLine("5. Suggested Project Ideas");
        sb.AppendLine("6. Expected Contribution");
        sb.AppendLine();

        sb.AppendLine("Partial analyses:");
        foreach (var analysis in partialAnalyses)
        {
            sb.AppendLine("----");
            sb.AppendLine(analysis);
        }

        return sb.ToString();
    }

}

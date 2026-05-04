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
""similarProjectLink"": ""string"",
""responsiblePeople"":""string""
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

    public static string BuildDepartmentChunkAnalysisPrompt(
        string context,
        string directorate,
        string? department)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an assistant specializing in corporate task analysis.");
        sb.AppendLine("Interpret the following records ONLY as a short analysis summary.");
        sb.AppendLine("Do not produce JSON. Do not produce project links. Do not use Markdown code blocks.");
        sb.AppendLine("Write only short bullet points.");
        sb.AppendLine();

        sb.AppendLine($"Directorate: {directorate}");

        if (!string.IsNullOrWhiteSpace(department))
            sb.AppendLine($"Department: {department}");

        sb.AppendLine();
        sb.AppendLine("Extract the following briefly:");
        sb.AppendLine("- Main task themes");
        sb.AppendLine("- Repeated tasks");
        sb.AppendLine("- AI/RPA/automation opportunities");
        sb.AppendLine("- Notable technical/process areas");
        sb.AppendLine();
        sb.AppendLine("Records:");
        sb.AppendLine(context);

        return sb.ToString();
    }


    public static string BuildFinalDepartmentAnalysisPrompt(
     IEnumerable<string> partialAnalyses,
     string directorate,
     string? department)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Sen kurumsal AI proje analizi yapan bir asistansın.");
        sb.AppendLine("Aşağıdaki parça analizlerini tek bir nihai sonuca dönüştür.");
        sb.AppendLine("Partial analizleri aynen kopyalama.");
        sb.AppendLine("İç içe JSON yazma.");
        sb.AppendLine("recommendation alanına JSON koyma.");
        sb.AppendLine("Markdown, ```json veya açıklama yazma.");
        sb.AppendLine("SADECE TEK BİR GEÇERLİ JSON NESNESİ DÖN.");
        sb.AppendLine();

        sb.AppendLine($"Direktörlük: {directorate}");

        if (!string.IsNullOrWhiteSpace(department))
            sb.AppendLine($"Departman: {department}");

        sb.AppendLine();
        sb.AppendLine("JSON şeması birebir şu olsun:");
        sb.AppendLine("""
{
  "task": "Departmanın/direktörlüğün ana görev özeti",
  "bestSolution": "AI",
  "automationRate": 85,
  "recommendation": "Kısa ve net öneri metni",
  "projectIdea": "Tek ve somut AI/RPA/otomasyon proje fikri",
  "similarProjectName": "Benzer gerçek ürün/proje adı",
  "similarProjectLink": "https://..."
}
""");

        sb.AppendLine();
        sb.AppendLine("Kurallar:");
        sb.AppendLine("- Tüm alanları dolu üret.");
        sb.AppendLine("- automationRate 0-100 arasında sayı olsun.");
        sb.AppendLine("- bestSolution sadece AI, RPA, Hybrid veya Other olsun.");
        sb.AppendLine("- projectIdea tek proje fikri olsun, liste olmasın.");
        sb.AppendLine("- similarProjectLink gerçek ürün/proje sayfası gibi görünmeli; bilmiyorsan 'Bulunamadı' yaz.");
        sb.AppendLine();

        sb.AppendLine("Parça analizleri:");
        foreach (var part in partialAnalyses)
        {
            sb.AppendLine("----");
            sb.AppendLine(part);
        }

        return sb.ToString();
    }
}

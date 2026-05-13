using System.Text;
using TaskAnalysis.Core.DTOs.DirectorateDTOs;

namespace TaskAnalysis.Service.Builders;

public static class AiPromptBuilder // Aynı işiyn çok benzerini yapan promtlar var düzeltilmeli 06.05.2026  
{
    public static string BuildNormalizeTasksPrompt(List<UniqueTaskDto> tasks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a data normalization expert.");
        sb.AppendLine("Your role is to analyze and group similar business tasks.");
        sb.AppendLine("Each line represents a single user.");
        sb.AppendLine("The 'AnaSorumluluk' column may contain multiple tasks.");
        sb.AppendLine("Do not treat the entire cell as one task.");
        sb.AppendLine("Instead, carefully examine and split the tasks inside each cell.");
        sb.AppendLine("Analyze each task individually.");
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
    } // Bu kod BuildUniqueTasksPrompt benzer refactoring ile burdan kaldırılamalı 05.05.2026

    public static string BuildUniqueTasksPrompt(List<UniqueTaskDto> tasks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an automation consultant.");
        sb.AppendLine("Analyze the following unique business tasks.");
        sb.AppendLine("For each task, propose a concrete automation project idea.");
        sb.AppendLine("Each line represents a single user.");
        sb.AppendLine("The 'AnaSorumluluk' column may contain multiple tasks.");
        sb.AppendLine("Do not treat the entire cell as one task.");
        sb.AppendLine("Instead, carefully examine and split the tasks inside each cell.");
        sb.AppendLine("Analyze each task individually.");

        sb.AppendLine("Determine the most appropriate solution type for each task without restriction.");

        sb.AppendLine("If none of these fit well, propose a new solution type and explain it.");
        sb.AppendLine("Estimate the automation rate (%) only if confident.");
        sb.AppendLine("Write recommendation text in Turkish.");
        sb.AppendLine("Try to suggest a realistic project based on the task.");
        sb.AppendLine("Only say 'No similar project found' if absolutely necessary.");

        sb.AppendLine();
        sb.AppendLine("Return ONLY valid JSON.");
        sb.AppendLine("Use English JSON keys exactly as specified.");

        sb.AppendLine();
        sb.AppendLine("Return ONLY valid JSON.");
        sb.AppendLine("No text outside JSON.");
        sb.AppendLine("No blank lines.");
        sb.AppendLine("Use this exact JSON structure and keys.");
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
        //sb.AppendLine("Each line is a separate task record.");
        sb.AppendLine("Each line represents the tasks of a single person and may contain multiple tasks.");
        sb.AppendLine(context);

        sb.AppendLine();
        sb.AppendLine("User question:");
        sb.AppendLine(question);

        sb.AppendLine();
        sb.AppendLine("Answer:");

        return sb.ToString();
    }

    public static string BuildDepartmentChunkAnalysisPrompt( string context, string directorate, string? department)
    { // Burda yapılan işlevi yukarda yapan fonk var zaten düzeltilmeli !!!!!!!!!! 05.05.2026
        var sb = new StringBuilder();

        sb.AppendLine("You are an assistant specializing in corporate task analysis.");
        sb.AppendLine("Interpret the following records ONLY as a short analysis summary.");
        sb.AppendLine("Do not produce JSON. Do not produce project links. Do not use Markdown code blocks.");
        sb.AppendLine("Write only short bullet points.");
        sb.AppendLine("Recommendation MUST be plain text. Do NOT embed JSON inside any field.");

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
        sb.AppendLine("Do not produce JSON inside any field.");
        sb.AppendLine("Recommendation MUST be plain text. Do NOT embed JSON inside any field.");

        return sb.ToString();
    }  

    /* BuildDepartmentChunkAnalysisPrompt → Kullanıcı sorusu yok, sadece görev kayıtlarını analiz edip kısa özet çıkarıyor. 
       Yani bir “analiz raporu” senaryosu.*/
    public static string BuildPersonAiAnalysisPrompt( string sicilNo, string fullName, string birim, string mudurluk, List<string> relevantChunks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Sen kurumsal süreçleri analiz eden bir yapay zeka dönüşüm danışmanısın.");
        sb.AppendLine("Aşağıda bir çalışana ait görev kayıtları verilmiştir.");
        sb.AppendLine();
        sb.AppendLine("Amacın:");
        sb.AppendLine("1. Her görev için AI ile yapılabilirlik yüzdesi üretmek.");
        sb.AppendLine("2. Her görev için en uygun çözüm tipini belirlemek.");
        sb.AppendLine("3. Her görev için kısa öneri yazmak.");
        sb.AppendLine("4. Her görev için uygulanabilir proje fikri üretmek.");
        sb.AppendLine("5. Kişinin toplam işlerinin yüzde kaçının AI ile desteklenebileceğini hesaplamak.");
        sb.AppendLine();
        sb.AppendLine("Kurallar:");
        sb.AppendLine("- Sadece JSON döndür.");
        sb.AppendLine("- JSON dışında açıklama yazma.");
        sb.AppendLine("- AiAutomationRate ve AverageAiAutomationRate 0 ile 100 arasında integer olmalı.");
        sb.AppendLine("- BestSolution değerleri şunlardan biri olabilir: AI, RPA, AI + RPA, Dashboard, Manuel, Hibrit.");
        sb.AppendLine("- Emin değilsen düşük değil makul oran ver.");
        sb.AppendLine("- Önerdiğin bir proje fikri somut ve uygulanabilir olmalı.");
        sb.AppendLine("- Önerdiğin bir proje fikrine uygun bir link ver: projectLink");
        sb.AppendLine("- Görevleri mümkün olduğunca ayrı ayrı analiz et.");
        sb.AppendLine();
        sb.AppendLine("Çalışan Bilgileri:");
        sb.AppendLine($"SicilNo: {sicilNo}");
        sb.AppendLine($"Ad Soyad: {fullName}");
        sb.AppendLine($"Birim: {birim}");
        sb.AppendLine($"Müdürlük: {mudurluk}");
        sb.AppendLine();
        sb.AppendLine("Görev Kayıtları:");
        sb.AppendLine("```");

        foreach (var chunk in relevantChunks)
        {
            sb.AppendLine(chunk);
            sb.AppendLine("---");
        }

        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("Aşağıdaki JSON formatına birebir uygun cevap ver:");
        sb.AppendLine("""
{
  "sicilNo": "string",
  "fullName": "string",
  "birim": "string",
  "mudurluk": "string",
  "totalTaskCount": 0,
  "averageAiAutomationRate": 0,
  "generalComment": "string",
  "taskAnalyses": [
    {
      "task": "string",
      "aiAutomationRate": 0,
      "bestSolution": "string",
      "recommendation": "string",
      "projectIdea": "string",
      "projectLink" : "string"
    }
  ]
}
""");

        return sb.ToString();
    }
    
    public static string BuildFinalDepartmentAnalysisPrompt( IEnumerable<string> partialAnalyses, string directorate, string? department)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an expert corporate AI automation analyst.");
        sb.AppendLine("Your job is to merge partial task analyses and produce ONE final department-level AI automation analysis.");
        sb.AppendLine();
        sb.AppendLine("CRITICAL OUTPUT RULES:");
        sb.AppendLine("- Return ONLY one valid JSON object.");
        sb.AppendLine("- Do NOT use Markdown.");
        sb.AppendLine("- Do NOT use ```json.");
        sb.AppendLine("- Do NOT add explanations outside JSON.");
        sb.AppendLine("- Do NOT nest JSON inside string fields.");
        sb.AppendLine("- All property names must exactly match the schema.");
        sb.AppendLine("- recommendation must be plain text only.");
        sb.AppendLine("- projectIdeas MUST be a JSON array.");
        sb.AppendLine("- projectIdeas MUST NOT be empty.");
        sb.AppendLine($"First, determine how many distinct tasks exist in the provided input.");
        sb.AppendLine($"Then, you MUST generate exactly the same number of project ideas as the number of distinct tasks you identified.");
        sb.AppendLine("There must be a strict one-to-one mapping:");
        sb.AppendLine("- 1 task = 1 AI analysis");
        sb.AppendLine("- 1 task = 1 project idea");
        sb.AppendLine("- 1 task = 1 similar project name");
        sb.AppendLine("- 1 task = 1 similar project link");
        sb.AppendLine("Do NOT generate fewer or more project ideas than the task count.");
        sb.AppendLine("- responsiblePeople MUST be a JSON array.");
        sb.AppendLine("- responsiblePeople must contain unique people only.");
        sb.AppendLine("- Do NOT repeat the same person twice.");
        sb.AppendLine();

        sb.AppendLine($"Directorate: {directorate}");

        if (!string.IsNullOrWhiteSpace(department))
            sb.AppendLine($"Department: {department}");

        sb.AppendLine();
        sb.AppendLine("JSON schema:");
        sb.AppendLine("""
        {
        "task": "Final summary of the directorate/department responsibilities",
        "bestSolution": "AI, RPA, Hybrid, or another suitable solution type",
        "automationRate": 0,
        "recommendation": "Plain text final recommendation",
        "projectIdeas": [
                      {
                        "task": "Concrete task/responsibility name",
                        "projectIdea": "Concrete automation or AI project idea",
                        "similarProjectName": "Similar real product/project name or Not Found",
                        "similarProjectLink": "https://... or Not Found"
                     }
                        ],
        "responsiblePeople": [
                    {
                      "name": "Person name",
                      "department": "Person department",
                      "reason": "Why this person is relevant"
                    }
                            ]
        }
""");
        sb.AppendLine("CRITICAL: Do NOT summarize the whole department as one task.");
        sb.AppendLine("CRITICAL: Split the responsibilities into separate distinct tasks.");
        sb.AppendLine("CRITICAL: If you identify 8 tasks, projectIdeas must contain exactly 8 items.");
        sb.AppendLine();
        sb.AppendLine("FIELD RULES:");
        sb.AppendLine("- Fill all fields.");
        sb.AppendLine("- automationRate must be a number between 0 and 100.");
        sb.AppendLine("- bestSolution must be AI, RPA, Hybrid, or a clearly named new solution type.");
        sb.AppendLine("- task must summarize the department's real responsibilities.");
        sb.AppendLine("- recommendation must explain what should be automated and why.");
        sb.AppendLine("- projectIdeas must contain exactly 5 items.");
        sb.AppendLine("- Every projectIdeas item must have task, projectIdea, similarProjectName, and similarProjectLink.");
        sb.AppendLine("- projectIdeas must be generated from the task, recommendation, department context, and partial analyses.");
        sb.AppendLine("- Even if partial analyses have empty projectIdeas, you MUST create 5 new concrete project ideas.");
        sb.AppendLine("- Each project idea must be directly related to this department's responsibilities.");
        sb.AppendLine("- Do NOT return [] for projectIdeas.");
        sb.AppendLine("- If you cannot find a real similar project, write Not Found.");
        sb.AppendLine("- responsiblePeople must include relevant people from partial analyses if available.");
        sb.AppendLine("- If no responsible people are available, return an empty array.");
        sb.AppendLine();

        sb.AppendLine("PARTIAL ANALYSES:");
        foreach (var part in partialAnalyses)
        {
            sb.AppendLine("-----");
            sb.AppendLine(part);
        }

        sb.AppendLine();
        sb.AppendLine("FINAL REMINDER:");
        sb.AppendLine("Return ONLY valid JSON.");
        sb.AppendLine("projectIdeas MUST contain exactly 5 items.");
        sb.AppendLine("projectIdeas MUST NOT be empty.");
        sb.AppendLine("Do not write any text before or after JSON.");

        return sb.ToString();
    }

    public static string BuildTaskChunkAnalysisPrompt( string directorate, string department, List<string> tasks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a corporate AI automation analyst.");
        sb.AppendLine("Analyze each task separately.");
        sb.AppendLine("Return ONLY valid JSON array.");
        sb.AppendLine("Do not use markdown.");
        sb.AppendLine("Do not use ```json.");
        sb.AppendLine();

        sb.AppendLine($"Directorate: {directorate}");
        sb.AppendLine($"Department: {department}");
        sb.AppendLine();

        sb.AppendLine("Tasks:");
        for (int i = 0; i < tasks.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {tasks[i]}");
        }
        sb.AppendLine("- IMPORTANT: For each chunk of records, extract ALL individual tasks mentioned in AnaSorumluluk fields.");
        sb.AppendLine("- Each numbered item in AnaSorumluluk is a separate task. Extract them individually.");
        sb.AppendLine("- Do not summarize multiple tasks into one. Return one JSON object per task.");
        sb.AppendLine();
        sb.AppendLine("Return JSON array exactly in this schema:");
        sb.AppendLine("""
[
  {
    "department": "Department name",
    "originalTask": "Original task text",
    "taskSummary": "Short clear Turkish summary of the task",
    "bestSolution": "AI, RPA, Hybrid, or another suitable solution type",
    "aiSupportRate": 0,
    "projectIdea": "One concrete AI/RPA/automation project idea for this task",
    "similarProjectName": "Similar real product/project name or Not Found",
    "similarProjectLink": "https://... or Not Found"
  }
]
""");

        sb.AppendLine();
        sb.AppendLine("Rules:");
        sb.AppendLine("- Return one JSON object for each task.");
        sb.AppendLine("- aiSupportRate must be between 0 and 100.");
        sb.AppendLine("- taskSummary must be Turkish.");
        sb.AppendLine("- projectIdea must be concrete and related to the task.");
        sb.AppendLine("- If no real similar project is known, use Not Found.");
        sb.AppendLine("- similarProjectLink must be a URL or Not Found.");
        sb.AppendLine("- Return only JSON array.");

        return sb.ToString();
    }

}
/* BuildTaskChunkAnalysisPrompt & BuildFinalDepartmentAnalysisPrompt
 🔗 Nasıl birlikte çalışıyorlar?
BuildTaskChunkAnalysisPrompt  
Bu fonksiyon, görevleri küçük parçalara (chunk) ayırıp AI’ye gönderiyor.

Her görev için ayrı JSON objesi üretilmesini sağlıyor.

Çıktı: Görev bazlı analizler (partial analyses).

BuildFinalDepartmentAnalysisPrompt  
Bu fonksiyon, chunk analizlerinden gelen parçalı sonuçları alıyor.

Hepsini birleştirip tek bir departman seviyesi final JSON oluşturuyor.

Çıktı: Departman için tek, bütünleşik analiz.

📌 Özet Akış
Görevler çıkarılır → chunk’lara bölünür.

BuildTaskChunkAnalysisPrompt → AI’ye gönderilir → her görev için JSON döner.

Bu JSON parçaları partialAnalyses listesine eklenir.

BuildFinalDepartmentAnalysisPrompt → partialAnalyses’i alır → tek bir final JSON üretir.

🎯 Amaç
Chunk prompt → detaylı görev bazlı analiz.

Final prompt → departman seviyesinde tek, temiz, kurallı JSON.

Yani evet, bunlar ortak çalışıyor: biri görevleri analiz ediyor, diğeri bu analizleri birleştirip final rapora dönüştürüyor.

👉 İstersen sana bu zinciri küçük bir örnekle gösterebilirim:

Chunk prompt → 3 görev için ayrı JSON döner.

Final prompt → bu 3 görevden tek bir departman analizi JSON üretir.
 */
/*Normalize → Unique → Chunk → Final → Chatbot  

Çalışma Akışı
1. Normalize Tasks Prompt
Fonksiyon: BuildNormalizeTasksPrompt

Amaç: Benzer görevleri birleştirmek (normalize etmek).

Çıktı:

JSON formatında:

json
[
{
"task": "Fatura kontrolü",
"departments": ["Muhasebe", "Finans"]
}
]
Kullanım: İlk adımda görev listesi temizleniyor ve aynı anlamdaki görevler tek satırda toplanıyor.
-----------------------------------------------------------------------------------------------------------------------------------------------
2. Unique Tasks Prompt
Fonksiyon: BuildUniqueTasksPrompt

Amaç: Normalize edilmiş görevleri tek tek analiz etmek.

Çıktı:

JSON formatında, her görev için:

json
[
{
"task": "Fatura kontrolü",
"departments": ["Muhasebe"],
"bestSolution": "RPA",
"automationRate": 80,
"recommendation": "Fatura kontrolü için RPA önerilir.",
"projectIdea": "Otomatik fatura doğrulama sistemi",
"similarProjectName": "SAP Invoice Management",
"similarProjectLink": "https://www.sap.com",
"responsiblePeople": "Muhasebe Uzmanı"
}
]
Kullanım: Her görev için otomasyon fikri, çözüm tipi, oran ve sorumlu kişi belirleniyor.
-----------------------------------------------------------------------------------------------------------------------------------------------
3. Department Chunk Analysis Prompt
Fonksiyon: BuildDepartmentChunkAnalysisPrompt

Amaç: Belirli bir müdürlük/departman için görev kayıtlarını kısa bullet point özetine dönüştürmek.

Çıktı:

Code
- Ana görev teması: Fatura kontrolü
- Tekrarlayan görevler: Personel işe alımı
- Otomasyon fırsatları: RPA ile fatura doğrulama
- Teknik alanlar: Eğitim süreçleri
Kullanım: Kullanıcıya hızlı özet sunmak için.

-----------------------------------------------------------------------------------------------------------------------------------------------
4. Final Department Analysis Prompt
Fonksiyon: BuildFinalDepartmentAnalysisPrompt
Amaç: Chunk analizlerinden gelen parçaları tek bir nihai JSON raporuna dönüştürmek.

Çıktı:

json
{
"task": "Muhasebe departmanının ana görevleri",
"bestSolution": "RPA",
"automationRate": 85,
"recommendation": "Fatura kontrolü süreçleri RPA ile hızlandırılmalı.",
"projectIdea": "Otomatik fatura doğrulama sistemi",
"similarProjectName": "SAP Invoice Management",
"similarProjectLink": "https://www.sap.com"
}
Kullanım: Pipeline’ın son adımı → tüm analizleri birleştirip tek bir standart JSON çıktısı üretir.
-----------------------------------------------------------------------------------------------------------------------------------------------
5. Chatbot Prompt
Fonksiyon: BuildChatbotPrompt

Amaç: Kullanıcının sorusunu verilen görev verisine dayanarak yanıtlamak.

Çıktı:

Code
Kullanıcı sorusu: Muhasebe departmanında hangi görevler var?
Cevap: Muhasebe departmanında fatura kontrolü görevi bulunmaktadır.
Kullanım: Q&A senaryosu → kullanıcıya doğrudan cevap verir.

📌 Genel Pipeline
Normalize → Görevleri temizle ve grupla.

Unique Analysis → Her görev için otomasyon fikri üret.

Chunk Analysis → Departman bazlı kısa özet çıkar.

Final Analysis → Parçalı analizleri tek JSON raporuna dönüştür.

Chatbot → Kullanıcı sorularını görev verisine dayanarak yanıtla.
*/
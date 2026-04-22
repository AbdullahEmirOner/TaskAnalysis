using System.Text;
using TaskAnalysis.Core.DTOs;

namespace TaskAnalysis.Service.Builders;

public static class AiPromptBuilder
{
    public static string BuildDirectoratePrompt(DirectorateSummaryDto summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an AI consultant specializing in corporate task analysis.");
        sb.AppendLine("Your role is to analyze directorate and department-level tasks and provide actionable insights.");
        sb.AppendLine();
        sb.AppendLine("Analyze the following directorate data.");
        sb.AppendLine("For each department, answer the following questions in detail:");
        sb.AppendLine("1. Identify which specific tasks can be performed with AI.");
        sb.AppendLine("2. For each task, estimate the automation rate (%) realistically.");
        sb.AppendLine("3. Estimate the weekly time workload (in hours) for each task.");
        sb.AppendLine("4. Propose concrete AI project ideas that could improve efficiency and reduce workload.");
        sb.AppendLine();
        sb.AppendLine("Output requirements:");
        sb.AppendLine("- Provide the answer ONLY in valid JSON.");
        sb.AppendLine("- Use English JSON keys exactly as specified.");
        sb.AppendLine("- Write all explanations, recommendations, and text values in Turkish.");
        sb.AppendLine("- Do not include any text outside the JSON.");
        sb.AppendLine();
        sb.AppendLine("Use this exact JSON structure:");
        sb.AppendLine("""
{
  "directorate": "string",
  "departments": [
    {
      "department": "string",
      "analyses": [
        {
          "task": "string",
          "aiSuitability": "string",
          "automationRate": 0,
          "estimatedWeeklyHours": 0,
          "recommendation": "string"
        }
      ]
    }
  ]
}
""");
        sb.AppendLine();
        sb.AppendLine($"Direktörlük: {summary.Direktorluk}");
        sb.AppendLine($"Toplam Kayıt Sayısı: {summary.ToplamKayitSayisi}");
        sb.AppendLine($"Müdürlük Sayısı: {summary.MudurlukSayisi}");
        sb.AppendLine();

        foreach (var mudurluk in summary.Mudurlukler)
        {
            sb.AppendLine($"Müdürlük: {mudurluk.Mudurluk}");
            sb.AppendLine($"Kayıt Sayısı: {mudurluk.KayitSayisi}");

            sb.AppendLine("Amaçlar:");
            foreach (var amac in mudurluk.Amaclar)
            {
                sb.AppendLine($"- {amac}");
            }

            sb.AppendLine("Yetkinlikler:");
            foreach (var yetkinlik in mudurluk.Yetkinlikler)
            {
                sb.AppendLine($"- {yetkinlik}");
            }

            sb.AppendLine("Ana Sorumluluklar:");
            foreach (var sorumluluk in mudurluk.AnaSorumluluklar)
            {
                sb.AppendLine($"- {sorumluluk}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}

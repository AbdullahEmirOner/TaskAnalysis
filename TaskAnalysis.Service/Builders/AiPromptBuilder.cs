using System.Text;
using TaskAnalysis.Core.DTOs;

namespace TaskAnalysis.Service.Builders;

public static class AiPromptBuilder
{
    public static string BuildDirectoratePrompt(DirectorateSummaryDto summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an AI consultant specializing in corporate task analysis.");
        sb.AppendLine("Analyze the following directorate data.");
        sb.AppendLine("For each department, answer the following questions:");
        sb.AppendLine("1. Which of these tasks can be performed with AI?");
        sb.AppendLine("2. For each task, what is the estimated automation rate (%)?");
        sb.AppendLine("3. What is the estimated weekly time workload for these tasks?");
        sb.AppendLine("4. What AI project ideas can be proposed to improve efficiency?");
        sb.AppendLine("Cevapları Türkçe üret.");
        sb.AppendLine();
        sb.AppendLine("Provide the answer in structured JSON format.");

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

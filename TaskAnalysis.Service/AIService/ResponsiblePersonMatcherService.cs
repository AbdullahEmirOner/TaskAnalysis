using TaskAnalysis.Core.DTOs.AIDTOs;
using TaskAnalysis.Core.Entities.CSVEntities;

//------------------------------------------------------------ Bu Görevden Kim Sorumlu? --------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------------------
public class ResponsiblePersonMatcherService : IResponsiblePersonMatcherService
{
    /* bu fonksiyonun doğru çalışması için görev metnindeki kelimelerin birebir geçmesi gerekiyor.
     * Çünkü embedding similarity yok, sadece keyword‑based Contains kontrolü var.
     */

    public List<ResponsiblePersonDto> FindResponsiblePeople(List<TaskRecord> records, string text, int take = 5)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<ResponsiblePersonDto>();

        var keywords = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        /* StringSplitOptions.RemoveEmptyEntries
Eğer arka arkaya birden fazla boşluk varsa, boş string ("") üretmez.

Örn: "Ali yerleri" (3 boşluk) → normalde ["Ali", "", "", "yerleri"] olurdu.

Ama RemoveEmptyEntries sayesinde → ["Ali", "yerleri"].
         */

        return records
            .Where(x => !string.IsNullOrWhiteSpace(x.ad_soyad))
            .Select(x => new
            {
                Record = x,
                Score = keywords.Count(k =>
                    (x.AnaSorumluluk?.ToLower().Contains(k) ?? false) ||
                    (x.Amac?.ToLower().Contains(k) ?? false) ||
                    (x.Yetki?.ToLower().Contains(k) ?? false))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(take)
            .Select(x => new ResponsiblePersonDto
            {
                Name = x.Record.ad_soyad,
                Department = x.Record.Mudurluk ?? "",
                Reason = $"Eşleşme skoru: {x.Score}"
            })
            .ToList();

        /* Bu fonksiyon, verilen görev metnindeki kelimeleri CSV kayıtlarındaki Amaç, Yetki, AnaSorumluluk alanlarıyla eşleştiriyor.
         * En yüksek eşleşme skoruna sahip kişileri bulup ResponsiblePersonDto listesi döndürüyor.
         * Yani senin sisteminde “bu görevden kim sorumlu?” sorusunun cevabını çıkarıyor.
         */
    }
}

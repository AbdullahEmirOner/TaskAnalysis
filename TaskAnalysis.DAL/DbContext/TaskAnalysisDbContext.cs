using Microsoft.EntityFrameworkCore;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Entities.RecordEntities;
using TaskAnalysis.Core.Interfaces.IDbContext;

namespace TaskAnalysis.DAL.DbContext
{
    public class TaskAnalysisDbContext : Microsoft.EntityFrameworkCore.DbContext, IApplicationDbContext
    {
        public TaskAnalysisDbContext(DbContextOptions<TaskAnalysisDbContext> options) : base(options)
        {
        }

        public DbSet<DepartmentAiAnalysisResult> DepartmentAiAnalysisResults { get; set; }
        /* DbSet<DepartmentAiAnalysisResult> ile EF Core’a bu entity’nin veritabanı karşılığı bir tablo olduğunu söylüyoruz.
           Migration çalışınca property’leri kolon olacak şekilde varsayılan olarak DepartmentAiAnalysisResults adlı tablo oluşturulur.
       
        --->DepartmentAiAnalysisResult sınıfını veritabanında takip et (track et), bunun için bir tablo oluştur/kullan
         */

        public DbSet<PersonAiAnalysisResult> PersonAiAnalysisResults { get; set; }

        public DbSet<DirectorateTaskAnalysisResult> DirectorateTaskAnalysisResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Burada EF Core’un varsayılan davranışını özelleştiriyoruz. ( EF, entity’lerden DB oluştururken benim ekstra kurallarım da var. )
        {
            base.OnModelCreating(modelBuilder);// Önce EF Core’un normal ayarlarını çalıştır.


            // Her direktörlük + departman çifti için sadece bir AI analiz sonucu olsun.
            modelBuilder.Entity<DepartmentAiAnalysisResult>() // DepartmentAiAnalysisResult entity’si için ayar yapacağım.
                .HasIndex(x => new { x.Directorate, x.Department }) // Directorate ve Department kolonlarına birlikte bir indeks oluştur. Bu, sorguların bu iki kolona göre daha hızlı çalışmasını sağlar.
                .IsUnique(); // Directorate ve Department kolonlarının birleşik olarak benzersiz (unique) olmasını istiyorum. Yani aynı müdürlük ve departman ikilisi birden fazla kez eklenemez.
            
            /*******************************************************************************\
            Index ---> Diskteki/veri sayfalarındaki kayda hızlı ulaşmak için oluşturulan rehber yapı.
            *******************************************************************************/
            modelBuilder.Entity<PersonAiAnalysisResult>() // PersonAiAnalysisResult entity’si için ayar yapacağım.
                .HasIndex(x => x.SicilNo) // SicilNo kolonuna bir indeks oluştur. Bu, sicil numarasına göre yapılan sorguların daha hızlı çalışmasını sağlar.
                .IsUnique(); // SicilNo kolonunun benzersiz (unique) olmasını istiyorum. Yani aynı sicil numarası birden fazla kez eklenemez.

            modelBuilder.Entity<DirectorateTaskAnalysisResult>() // DirectorateTaskAnalysisResult entity’si için ayar yapacağım.
                .HasIndex(x => x.Directorate)
                .IsUnique(); // Directorate kolonunun benzersiz (unique) olmasını istiyorum. Yani aynı müdürlük birden fazla kez eklenemez.

            modelBuilder.Entity<DirectorateTaskAnalysisResult>()
                .Property(x => x.ResultJson) // ResultJson kolonunun veri tipini nvarchar(max) yap. (SQL Server’da uzun metinler için kullanılır.)
                .HasColumnType("nvarchar(max)"); // EF Core varsayılan olarak string tiplerini nvarchar(450) yapar, bu da uzun JSON metinleri için yeterli değildir. Bu ayarla ResultJson kolonunun uzun metinleri desteklemesini sağlıyoruz.
        
        }
    }
}

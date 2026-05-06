using TaskAnalysis.Core.Interfaces;
using OTOKAR.TaskAnalysis.DAL.Readers;
using TaskAnalysis.Service.AIService;
using TaskAnalysis.Service.Helpers;
using TaskAnalysis.Service.LangChainService;
using TaskAnalysis.Service.Mini_LangChainService;

namespace TaskAnalysis.API.Extesions
{
    public static class Extensions
    {
        public static void AddCsvTaskReaders(this IServiceCollection services) =>
    services.AddScoped<ICsvReaderService, CsvTaskReaders>();

        public static void AddRetrievalService(this IServiceCollection services) =>
services.AddScoped<IRetrievalService, RetrievalService>();
        
        public static void AddEmbeddingService(this IServiceCollection services) =>
services.AddScoped<IEmbeddingService, EmbeddingService>();

        public static void AddEmbeddingHelperService(this IServiceCollection services) =>
services.AddScoped<IEmbeddingHelperService, EmbeddingHelperService>();

        public static void AddAnalysisService(this IServiceCollection services) =>
services.AddScoped<IAnalysisService, AnalysisService>();

        public static void AddVectorDbService(this IServiceCollection services) =>
services.AddSingleton<IVectorDbService, VectorDbService>();
        /*builder.Services.AddScoped<IVectorDbService, VectorDbService>(); Indexlemeyi her seferinde oluşturduğu için AddScoped her yerine  AddSingleton kuyllandım
        //builder.Services.AddSingleton<IVectorDbService, VectorDbService>(); //Indexlemeyi her seferinde oluşturduğu için AddScoped yerine AddSingleton kuyllandım
        /*Tür	             Davranış
        Scoped    ?	Her requestte yeni memory
        Transient ?	Her çağrıda sıfır
        Singleton ?	Tek memory, her yerde aynı
         */
        public static void AddAiService(this IServiceCollection services) =>
services.AddHttpClient<IAiService, AiService>();

        public static void AddResponsiblePersonMatcherService(this IServiceCollection services) =>
services.AddScoped<IResponsiblePersonMatcherService, ResponsiblePersonMatcherService>();

        public static void AddPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowNetlify", policy =>
                {
                    policy.WithOrigins("https://gorevtn.netlify.app",
                        "https://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
        }
    }
}
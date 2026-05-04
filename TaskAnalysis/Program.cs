using TaskAnalysis.Core.Interfaces;
using TaskAnalysis.DAL.Readers;
using TaskAnalysis.Service;
using TaskAnalysis.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<ICsvReaderService, CsvTaskReaders>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
//builder.Services.AddScoped<IVectorDbService, VectorDbService>(); Indexlemeyi her seferinde oluþturduðu için AddScoped her yerine  AddSingleton kuyllandým
builder.Services.AddSingleton<IVectorDbService, VectorDbService>(); //Indexlemeyi her seferinde oluþturduðu için AddScoped yerine AddSingleton kuyllandým
/*Tür	             Davranýþ
Scoped    ?	Her requestte yeni memory
Transient ?	Her çaðrýda sýfýr
Singleton ?	Tek memory, her yerde ayný
 */
builder.Services.AddHttpClient<IAiService, AiService>();
builder.Services.AddScoped<IResponsiblePersonMatcherService, ResponsiblePersonMatcherService>();
//builder.Services.AddHttpClient<IAiService, AiService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNetlify", policy =>
    {
        policy.WithOrigins("https://gorevtn.netlify.app",
            "https://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("AllowNetlify");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var indexService = scope.ServiceProvider.GetRequiredService<IAnalysisService>();
    await indexService.IndexAllCsvAsync();
}

app.MapControllers();

app.Run();



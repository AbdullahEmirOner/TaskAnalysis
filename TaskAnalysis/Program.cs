using TaskAnalysis.API.Extesions;
using TaskAnalysis.Core.Interfaces.IDbContext;
using TaskAnalysis.Core.Interfaces.IRAG;
using TaskAnalysis.DAL.DbContext;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddCsvTaskReaders();
builder.Services.AddRetrievalService();
builder.Services.AddEmbeddingService();
builder.Services.AddEmbeddingHelperService();
builder.Services.AddAnalysisService();
builder.Services.AddTaskExtractionService();
builder.Services.AddVectorDbService(); 
builder.Services.AddAiService();
builder.Services.AddResponsiblePersonMatcherService();
builder.Services.AddPolicy();
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<TaskAnalysisDbContext>());
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowNetlify");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var indexService = scope.ServiceProvider.GetRequiredService<IRetrievalService>();
    await indexService.IndexAllCsvAsync();
}

app.MapControllers();

app.Run();



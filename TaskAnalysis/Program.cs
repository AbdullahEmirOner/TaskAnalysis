using TaskAnalysis.API.Extesions;
using TaskAnalysis.Core.Interfaces;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCsvTaskReaders();
builder.Services.AddRetrievalService();
builder.Services.AddEmbeddingService();
builder.Services.AddEmbeddingHelperService();
builder.Services.AddAnalysisService();
builder.Services.AddVectorDbService();
builder.Services.AddAiService();
builder.Services.AddResponsiblePersonMatcherService();
builder.Services.AddPolicy();   

builder.Services.AddMemoryCache();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowNetlify");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var indexService = scope.ServiceProvider.GetRequiredService<IRetrievalService>();
    await indexService.IndexAllCsvAsync();
}

app.MapControllers();

app.Run();



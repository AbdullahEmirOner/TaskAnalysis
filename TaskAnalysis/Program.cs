using TaskAnalysis.Core.Interfaces;
using TaskAnalysis.DAL.Readers;
using TaskAnalysis.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICsvReaderService, CsvTaskReaders>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

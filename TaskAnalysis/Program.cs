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
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddHttpClient<IAiService, AiService>();

//builder.Services.AddHttpClient<IAiService, AiService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000",
            "https://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthorization();
app.MapControllers();
app.Run();

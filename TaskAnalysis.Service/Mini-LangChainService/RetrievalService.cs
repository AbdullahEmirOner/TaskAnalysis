using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.Entities;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.Mini_LangChainService
{
    public class RetrievalService : IRetrievalService
    {
        private readonly Dictionary<string, List<(string Text, float[] Vector)>> _vectorStore = new();
        private readonly ICsvReaderService _csvReaderService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IAiService _aiService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IVectorDbService _vectorDb;

        public RetrievalService(IVectorDbService vectorDbService, IEmbeddingService embeddingService,
            ICsvReaderService csvReaderService, IAiService aiService, IConfiguration configuration, IMemoryCache cache)
        {
            _csvReaderService = csvReaderService;
            _aiService = aiService;
            _configuration = configuration;
            _cache = cache;
            _embeddingService = embeddingService;
            _vectorDb = vectorDbService;
        }
      
        public async Task<object> IndexAllCsvAsync()
        {
            var folderPath = _configuration["CsvSettings:FolderPath"];

            if (string.IsNullOrWhiteSpace(folderPath))
                throw new Exception("CSV klasör yolu tanımlı değil.");

            if (!Directory.Exists(folderPath))
                throw new Exception("CSV klasörü bulunamadı.");

            var files = Directory.GetFiles(folderPath, "*.csv");

            var results = new List<object>();

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);

                try
                {
                    var result = await IndexCsvAsync(fileName);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        fileName,
                        indexed = false,
                        error = ex.Message
                    });
                }
            }

            return new
            {
                indexedFileCount = results.Count(x =>
                    x.GetType().GetProperty("indexed")?.GetValue(x)?.Equals(true) == true),
                totalFileCount = files.Length,
                files = results,
                message = "CSV indexleme işlemi tamamlandı."
            };
        }

        public bool IsValidText(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /* public List<string> ChunkRecords(List<TaskRecord> records)
          {
              return records.Select(r =>
              $"Direktörlük: {r.Mudurluk} | Birim: {r.Birim} | Amaç: {r.Amac} | Ana Sorumluluk: {r.AnaSorumluluk}"
              ).ToList();
          }*/
        //   SicilNo;Birim;Mudurluk;Amac;Yetki;AnaSorumluluk

        public async Task<object> IndexCsvAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new Exception("CSV dosya adı boş olamaz.");

            var folderPath = _configuration["CsvSettings:FolderPath"];

            if (string.IsNullOrWhiteSpace(folderPath))
                throw new Exception("CSV klasör yolu tanımlı değil.");

            var safeFileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(folderPath, safeFileName);

            if (!File.Exists(filePath))
                throw new Exception("CSV dosyası bulunamadı.");

            var records = _csvReaderService.ReadCsv(filePath);

            if (records == null || records.Count == 0)
                throw new Exception("CSV okundu ama kayıt bulunamadı.");

            var chunks = CreateChunks(records, 20);

            _vectorDb.Clear(safeFileName);

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.CreateEmbeddingAsync(chunk);
                await _vectorDb.InsertAsync(safeFileName, chunk, embedding);
            }

            return new
            {
                fileName = safeFileName,
                indexed = true,
                recordCount = records.Count,
                chunkCount = chunks.Count,
                message = "CSV başarıyla memory vector store içine indexlendi."
            };
        }

        public List<string> CreateChunks(List<TaskRecord> records, int chunkSize = 20)
        {
            var chunks = new List<string>();

            for (int i = 0; i < records.Count; i += chunkSize)
            {
                var group = records.Skip(i).Take(chunkSize).ToList();

                var chunk = string.Join("\n", group.Select((r, index) =>
                    $"Kayıt: {i + index + 1} | " +
                    $"Müdürlük: {r.Mudurluk} | " +
                    $"Birim: {r.Birim} | " +
                    $"Amaç: {r.Amac} | " +
                    $"Yetki: {r.Yetki} | " +
                    $"Ana Sorumluluk: {r.AnaSorumluluk}"
                ));

                chunks.Add(chunk);
            }

            return chunks;
        }

        /*  private double CosineSimilarity(float[] v1, float[] v2)
          {
              var dot = v1.Zip(v2, (a, b) => a * b).Sum();
              var mag1 = Math.Sqrt(v1.Sum(x => x * x));
              var mag2 = Math.Sqrt(v2.Sum(x => x * x));

              return dot / (mag1 * mag2 + 1e-8);
          }*/

        public async Task<List<string>> RetrieveRelevantChunks(string fileName, string question)
        {
            if (!_vectorStore.ContainsKey(fileName))
                return new List<string>();

            var questionEmbedding = await _embeddingService.CreateEmbeddingAsync(question);

            var scored = _vectorStore[fileName]
                .Select(v => new
                {
                    v.Text,
                    Score = _vectorDb.CosineSimilarity(v.Vector, questionEmbedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Text)
                .ToList();

            return scored;
        }


    }
}

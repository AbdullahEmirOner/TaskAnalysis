using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.Services;

public class VectorDbService : IVectorDbService
{
    private readonly Dictionary<string, List<VectorItem>> _store = new();

    public Task InsertAsync(string fileName, string text, float[] embedding)
    {
        var safeFileName = Path.GetFileName(fileName);

        if (!_store.ContainsKey(safeFileName))
            _store[safeFileName] = new List<VectorItem>();

        _store[safeFileName].Add(new VectorItem
        {
            Text = text,
            Embedding = embedding
        });

        return Task.CompletedTask;
    }

    public Task<List<string>> SearchAsync(string fileName, float[] embedding, int limit = 3)
    {
        var safeFileName = Path.GetFileName(fileName);

        if (!_store.ContainsKey(safeFileName))
            return Task.FromResult(new List<string>());

        var results = _store[safeFileName]
            .Select(x => new
            {
                x.Text,
                Score = CosineSimilarity(x.Embedding, embedding)
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => x.Text)
            .ToList();

        return Task.FromResult(results);
    }

    public bool IsIndexed(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);

        return _store.ContainsKey(safeFileName)
               && _store[safeFileName].Count > 0;
    }

    public void Clear(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);

        if (_store.ContainsKey(safeFileName))
            _store.Remove(safeFileName);
    }

    private static double CosineSimilarity(float[] v1, float[] v2)
    {
        var length = Math.Min(v1.Length, v2.Length);

        double dot = 0;
        double mag1 = 0;
        double mag2 = 0;

        for (int i = 0; i < length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-8);
    }

    private class VectorItem
    {
        public string Text { get; set; } = string.Empty;

        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}

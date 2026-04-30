using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.Services;

public class EmbeddingService : IEmbeddingService
{
    private const int VectorSize = 256;

    public Task<float[]> CreateEmbeddingAsync(string text)
    {
        var vector = new float[VectorSize];

        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult(vector);

        var words = text
            .ToLowerInvariant()
            .Replace(".", " ")
            .Replace(",", " ")
            .Replace(";", " ")
            .Replace(":", " ")
            .Replace("/", " ")
            .Replace("\\", " ")
            .Replace("(", " ")
            .Replace(")", " ")
            .Replace("\"", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            var index = Math.Abs(GetStableHash(word)) % VectorSize;
            vector[index] += 1;
        }

        Normalize(vector);

        return Task.FromResult(vector);
    }

    private static int GetStableHash(string value)
    {
        unchecked
        {
            var hash = 23;

            foreach (var c in value)
                hash = hash * 31 + c;

            return hash;
        }
    }

    private static void Normalize(float[] vector)
    {
        double sum = 0;

        foreach (var value in vector)
            sum += value * value;

        var magnitude = Math.Sqrt(sum);

        if (magnitude == 0)
            return;

        for (int i = 0; i < vector.Length; i++)
            vector[i] = (float)(vector[i] / magnitude);
    }
}

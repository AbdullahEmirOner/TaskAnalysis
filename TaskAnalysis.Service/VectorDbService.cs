using Qdrant.Client;
using Qdrant.Client.Grpc;
using TaskAnalysis.Core.Interfaces;

public class VectorDbService : IVectorDbService
{
    private readonly QdrantClient _client;
    private const string CollectionName = "tasks";

    public VectorDbService()
    {
        _client = new QdrantClient("localhost", 6333);
    }

    public async Task InitAsync()
    {
        await _client.CreateCollectionAsync(CollectionName, new VectorParams
        {
            Size = 1536,
            Distance = Distance.Cosine
        });
    }

    public async Task InsertAsync(string text, float[] embedding)
    {
        await _client.UpsertAsync(CollectionName, new[]
        {
            new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = embedding,
                Payload =
                {
                    ["text"] = text
                }
            }
        });
    }

    public async Task<List<string>> SearchAsync(float[] embedding)
    {
        var result = await _client.SearchAsync(CollectionName, embedding, limit: 5);

        return result.Select(r => r.Payload["text"].StringValue).ToList();
    }
}

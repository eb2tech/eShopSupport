using Microsoft.Extensions.AI;
using SmartComponents.LocalEmbeddings.SemanticKernel;

namespace eShopSupport.Backend.Services;

public class LocalEmbeddingGeneratorAdapter : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly LocalTextEmbeddingGenerationService _service = new();

    public EmbeddingGeneratorMetadata Metadata => new("local-embeddings");

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var embeddings = await _service.GenerateEmbeddingsAsync(values.ToList());
        var results = embeddings.Select(e => new Embedding<float>(new ReadOnlyMemory<float>(e.ToArray()))).ToList();
        return new GeneratedEmbeddings<Embedding<float>>(results);
    }

    public void Dispose()
    {
        _service.Dispose();
    }

    public object? GetService(Type serviceType, object? key = null)
    {
        return serviceType == typeof(IEmbeddingGenerator<string, Embedding<float>>) ? this : null;
    }
}

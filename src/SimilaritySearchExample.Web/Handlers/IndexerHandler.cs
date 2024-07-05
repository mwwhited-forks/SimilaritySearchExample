using Eliassen.AI;
using Eliassen.Documents;
using Eliassen.Documents.Models;
using Eliassen.MessageQueueing;
using Eliassen.MessageQueueing.Services;
using Eliassen.Search;
using Eliassen.System.Security.Cryptography;
using SimilaritySearchExample.Web.Models;
using System.Runtime.InteropServices;

namespace SimilaritySearchExample.Web.Handlers;

public class IndexerHandler : IMessageQueueHandler<Indexer>
{
    private readonly IBlobContainer<TextResources> _text;
    private readonly IBlobContainer<Summaries> _summaries;
    private readonly ILogger _logger;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IHash _hash;
    private readonly IVectorStore<DocumentKeys> _vector;

    public IndexerHandler(
        IBlobContainer<TextResources> text,
        IBlobContainer<Summaries> summaries,
        ILogger<IndexerHandler> logger,
        IEmbeddingProvider embeddingProvider,
        IHash hash,
        IVectorStore<DocumentKeys> vector
        )
    {
        _text = text;
        _summaries = summaries;
        _logger = logger;
        _embeddingProvider = embeddingProvider;
        _hash = hash;
        _vector = vector;
    }

    public Task HandleAsync(object message, IMessageContext context) =>
        message switch
        {
            ResourceReference r => HandleAsync(r, context),
            _ => Task.CompletedTask
        };

    public async Task HandleAsync(ResourceReference message, IMessageContext context)
    {
        _logger.LogInformation("Get: \"{filename}\" from \"{source}\"", message.FileName, message.Source);
        var content = message.Source switch
        {
            nameof(TextResources) => await _text.GetContentAsync(message.FileName),
            nameof(Summaries) => await _summaries.GetContentAsync(message.FileName),
            _ => null,
        };

        if (content == null)
        {
            _logger.LogWarning("No indexable content for {Source}: {FileName}", message.Source, message.FileName);
            return;
        }

        var hash = _hash.GetHash(message.FileName);

        var reader = new StreamReader(content.Content);
        var text = await reader.ReadToEndAsync();
        var memory = text.AsMemory();

        //TODO: do this better

        var chunkLength = _embeddingProvider.Length;
        var chunks = new List<ReadOnlyMemory<float>>();
        for (var x = 0; x < text.Length; x += chunkLength)
        {
            var chunk = memory[x..(x + Math.Min(chunkLength, text.Length - x))];

            if (MemoryMarshal.TryGetString(chunk, out var value, out var st, out var len))
            {
                var embedding = await _embeddingProvider.GetEmbeddingAsync(value);
                chunks.Add(embedding);
            }
        }

        if (chunks.Count > 0)
        {
            var results = await _vector.StoreVectorsAsync(chunks, new()
            {
                {nameof(message.ContentType), message.ContentType },
                {nameof(message.FileName), message.FileName },
                {nameof(message.Source), message.Source },
                {"Hash", hash },
            });

            var request = new ContentMetaDataReference
            {
                FileName = message.FileName,
                ContentType = message.ContentType,
                MetaData = new()
            {
                { "Hash",hash },
                { "EmbeddingIds",string.Join(";",results) },
            },
            };

            var result = message.Source switch
            {
                nameof(TextResources) => await _text.StoreContentMetaDataAsync(request),
                nameof(Summaries) => await _summaries.StoreContentMetaDataAsync(request),
                _ => throw new NotSupportedException(),
            };
            _logger.LogInformation("Index: \"{filename}\" from \"{source}\" -> {indexes} ({result})", message.FileName, message.Source, results, result);
        }
        else
        {
            _logger.LogWarning("No chunks content for {Source}: {FileName}", message.Source, message.FileName);
        }
    }
}

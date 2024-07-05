using Eliassen.AI;
using Eliassen.Common;
using Eliassen.Documents;
using Eliassen.Documents.Models;
using Eliassen.Search;
using Eliassen.System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimilaritySearchExample.Persistence;
using SimilaritySearchExample.Web.Handlers;
using SimilaritySearchExample.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SimilaritySearchExample.Tests.Handlers;

[TestClass]
public class IVectorStoreTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task Test()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["QdrantOptions:Url"] = "http://127.0.0.1:6334",
                ["QdrantOptions:EnsureCollectionExists"] = "true",

                ["SentenceEmbeddingOptions:Url"] = "http://127.0.0.1:5080",

                ["AzureBlobProviderOptions:ConnectionString"] = "DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;",
                ["AzureBlobProviderOptions:EnsureContainerExists"] = "true",
            })
            .Build();
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddEventQueueHandlers()
            .AddApplicationDatabase(config)
            .TryAllCommonExtensions(config)
            .BuildServiceProvider();
        ;

        var vector = services.GetRequiredService<IVectorStore<DocumentKeys>>();
        var blobs = services.GetRequiredService<IBlobContainer<Summaries>>();
        var embeddingProvider = services.GetRequiredService<IEmbeddingProvider>();

        var message = new
        {
            ContentType = "ContentType",
            FileName = "FileName",
            Source = "Source",
        };
        var hash = Guid.NewGuid().ToString();

        var text = new string(Enumerable.Range(0, embeddingProvider.Length * 5).Select(i => (char)(i % 26 + 'a')).ToArray());
        var memory = text.AsMemory();

        var chunkLength = embeddingProvider.Length;
        var chunks = new List<(ReadOnlyMemory<float>, Dictionary<string, object>)>();
        var segment = 0;
        for (var x = 0; x < text.Length; x += chunkLength)
        {
            var chunk = memory[x..(x + Math.Min(chunkLength, text.Length - x))];
            var value = chunk.ToString();

            var embedding = await embeddingProvider.GetEmbeddingAsync(value);
            chunks.Add((embedding, new()
            {
                ["Segment"] = segment++,
                ["Length"] = chunk.Length,
                ["Start"] = x,
            }));
        }

        if (chunks.Count > 0)
        {
            var results = await vector.StoreVectorsAsync(chunks, new()
            {
                [nameof(message.ContentType)] = message.ContentType,
                [nameof(message.FileName)] = message.FileName,
                [nameof(message.Source)] = message.Source,
                ["Hash"] = hash,
            });
        }
    }
}

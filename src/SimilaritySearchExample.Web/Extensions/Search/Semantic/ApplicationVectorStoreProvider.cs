//// Ignore Spelling: metadata

//using Eliassen.Search.Models;
//using Eliassen.Search.Semantic;
//using Microsoft.EntityFrameworkCore;
//using Pgvector;
//using Pgvector.EntityFrameworkCore;
//using ResourceProjectDatabase;

//namespace ResourceProfiler.Web.Extensions.Search.Semantic;

//public class ApplicationVectorStoreProviderFactory : IVectorStoreProviderFactory
//{
//    private readonly IServiceProvider _serviceProvider;

//    public ApplicationVectorStoreProviderFactory(IServiceProvider serviceProvider) =>
//        _serviceProvider = serviceProvider;

//    public IVectorStoreProvider Create(string containerName) =>
//        ActivatorUtilities.CreateInstance<ApplicationVectorStoreProvider>(_serviceProvider, containerName);
//}

//public class ApplicationVectorStoreProvider : IVectorStoreProvider
//{
//    private readonly ResourceProfilerContext _db;
//    public ApplicationVectorStoreProvider(
//        ResourceProfilerContext db,
//        string collectionName
//        )
//    {
//        _db = db;
//        CollectionName = collectionName;
//    }

//    public string CollectionName { get; set; }

//    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find)
//    {
//        var query = from vector in _db.Vectors
//                    let score = vector.Embedding!.CosineDistance(new Vector(find))
//                    orderby score descending
//                    select new SearchResultModel
//                    {
//                        ItemId = vector.Id.ToString(),
//                        Score = Convert.ToSingle(score),
//                    };

//        await foreach (var item in query.AsAsyncEnumerable())
//            yield return item;
//    }

//    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy)
//    {
//        var query = from vector in _db.Vectors
//                    orderby vector.Embedding!.L2Distance(new Vector(find))
//                    select new SearchResultModel
//                    {
//                        ItemId = vector.Id.ToString(),
//                        Score = -1,
//                    };

//        await foreach (var item in query.AsAsyncEnumerable())
//        {
//            yield return item;
//        }
//    }

//    public async IAsyncEnumerable<SearchResultModel> ListAsync()
//    {
//        var query = from vector in _db.Vectors
//                    select new SearchResultModel
//                    {
//                        ItemId = vector.Id.ToString(),
//                        Score = -1,
//                    };

//        await foreach(var item in query.AsAsyncEnumerable())
//        {
//            yield return item;
//        }
//    }

//    public async Task<string[]> StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, Dictionary<string, object> metadata)
//    {
//        var entities = (from embeding in embeddings
//                        select new DocumentVector
//                        {
//                            Embedding = new Pgvector.Vector(embeding),
//                            Data = (from data in metadata
//                                    select new DocumentData
//                                    {
//                                        Name = data.Key,
//                                        Value = data.Value?.ToString() ?? "",
//                                    }).ToList(),
//                        }).ToList();

//        _db.AddRange(entities);
//        await _db.SaveChangesAsync();

//        var ids = entities.Select(e => e.Id.ToString()).ToArray();
//        return ids;
//    }
//}

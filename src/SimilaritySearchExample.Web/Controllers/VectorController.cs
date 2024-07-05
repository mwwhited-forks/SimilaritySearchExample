using Eliassen.AI;
using Eliassen.Search;
using Eliassen.Search.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceProfiler.Web.Models;
using System.Net;

namespace ResourceProfiler.Web.Controllers;

[AllowAnonymous]
[Route("[Controller]/[Action]")]
public class VectorController : Controller
{
    private readonly IVectorStore<DocumentKeys> _vectorStore;
    private readonly IEmbeddingProvider _embedding;

    public VectorController(
        IVectorStore<DocumentKeys> vectorStore,
        IEmbeddingProvider embedding
        )
    {
        _vectorStore = vectorStore;
        _embedding = embedding;
    }

    [HttpGet, HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IAsyncEnumerable<SearchResultModel> List() =>
        _vectorStore.ListAsync();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IAsyncEnumerable<SearchResultModel> Query([FromBody] float[] model) =>
        _vectorStore.FindNeighborsAsync(model);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IAsyncEnumerable<SearchResultModel> QueryGrouped([FromBody] float[] model, string groupBy) =>
        _vectorStore.FindNeighborsAsync(model, groupBy);

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async IAsyncEnumerable<SearchResultModel> Search(string query)
    {
        var embedding = await _embedding.GetEmbeddingAsync(query);
        await foreach (var item in _vectorStore.FindNeighborsAsync(embedding))
            yield return item;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async IAsyncEnumerable<SearchResultModel> SearchGrouped(string query, string groupBy)
    {
        var embedding = await _embedding.GetEmbeddingAsync(query);
        await foreach (var item in _vectorStore.FindNeighborsAsync(embedding, groupBy))
            yield return item;
    }
}

using Eliassen.AI;
using Eliassen.Search;
using Eliassen.Search.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimilaritySearchExample.Web.Models;
using System.Net;

namespace SimilaritySearchExample.Web.Controllers;

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

    /// <summary>
    /// list all vectors in the vector database
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IAsyncEnumerable<SearchResultModel> List() =>
        _vectorStore.ListAsync();

    /// <summary>
    /// find nearest neighbor by vector value
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IAsyncEnumerable<SearchResultModel> Query([FromBody] float[] model) =>
        _vectorStore.FindNeighborsAsync(model);

    /// <summary>
    /// find nearest content in from vector store
    /// </summary>
    /// <param name="model"></param>
    /// <param name="groupBy">suggest &quot;Hash&quot;</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IAsyncEnumerable<SearchResultModel> QueryGrouped([FromBody] float[] model, string groupBy) =>
        _vectorStore.FindNeighborsAsync(model, groupBy);

    /// <summary>
    /// perform embedding and lookup neighbors from vector store
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async IAsyncEnumerable<SearchResultModel> Search(string query)
    {
        var embedding = await _embedding.GetEmbeddingAsync(query);
        await foreach (var item in _vectorStore.FindNeighborsAsync(embedding))
            yield return item;
    }

    /// <summary>
    /// perform embedding and lookup neighbors from vector store
    /// </summary>
    /// <param name="query"></param>
    /// <param name="groupBy">suggest &quot;Hash&quot;</param>
    /// <returns></returns>
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

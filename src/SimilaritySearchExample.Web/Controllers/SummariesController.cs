using Eliassen.Documents;
using Eliassen.Documents.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimilaritySearchExample.Web.Models;
using System.Net;

namespace SimilaritySearchExample.Web.Controllers;

[AllowAnonymous]
[Route("[Controller]/[Action]")]
public class SummariesController : Controller
{
    private readonly IBlobContainer<Summaries> _content;

    public SummariesController(
        IBlobContainer<Summaries> content
        ) => _content = content;

    /// <summary>
    /// list document summaries from the blob store
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpPost]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IQueryable<ContentMetaDataReference> List() => _content.QueryContent();

    /// <summary>
    /// download summary from blob store by file name
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Download(string file) =>
        await _content.GetContentAsync(file) switch
        {
            null => NotFound(),
            ContentReference blob => File(blob.Content, blob.ContentType, blob.FileName)
        };

    /// <summary>
    /// remove summary from blob store
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task Delete(string file) =>
        await _content.DeleteContentAsync(file);

    /// <summary>
    /// add metadata to entry in blob store
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Store(
        [FromForm] ContentMetaDataReference model
        ) =>
        ModelState.IsValid && await _content.StoreContentMetaDataAsync(model) ? Ok() : BadRequest(ModelState);
}

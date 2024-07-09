using Eliassen.Documents;
using Eliassen.Documents.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimilaritySearchExample.Web.Models;
using System.Net;

namespace SimilaritySearchExample.Web.Controllers;

[AllowAnonymous]
[Route("[Controller]/[Action]")]
public class TextDocumentController : Controller
{
    private readonly IBlobContainer<TextResources> _content;

    public TextDocumentController(
        IBlobContainer<TextResources> content
        ) => _content = content;

    /// <summary>
    /// list text contented content from the blob store
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpPost]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IQueryable<ContentMetaDataReference> List() => _content.QueryContent();

    /// <summary>
    /// download text content from the blob store
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
    /// remove text content from the blob store
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task Delete(string file) =>
        await _content.DeleteContentAsync(file);

    /// <summary>
    /// add metadata to the text content in the blob store
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

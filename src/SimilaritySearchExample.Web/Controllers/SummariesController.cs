using Eliassen.Documents;
using Eliassen.Documents.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceProfiler.Web.Models;
using System.Net;

namespace Eliassen.WebApi.Controllers;

[AllowAnonymous]
[Route("[Controller]/[Action]")]
public class SummariesController : Controller
{
    private readonly IBlobContainer<Summaries> _content;

    public SummariesController(
        IBlobContainer<Summaries> content
        ) => _content = content;

    [HttpGet, HttpPost]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IQueryable<ContentMetaDataReference> List() => _content.QueryContent();

    [HttpGet]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Download(string file) =>
        await _content.GetContentAsync(file) switch
        {
            null => NotFound(),
            ContentReference blob => File(blob.Content, blob.ContentType, blob.FileName)
        };

    [HttpDelete]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task Delete(string file) =>
        await _content.DeleteContentAsync(file);

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Store(
        [FromForm] ContentMetaDataReference model
        ) =>
        ModelState.IsValid && await _content.StoreContentMetaDataAsync(model) ? Ok() : BadRequest(ModelState);
}

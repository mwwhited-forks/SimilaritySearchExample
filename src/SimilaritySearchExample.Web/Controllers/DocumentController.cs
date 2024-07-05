using Eliassen.Documents;
using Eliassen.Documents.Models;
using Eliassen.MessageQueueing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceProfiler.Web.Models;
using System.Net;

namespace Eliassen.WebApi.Controllers;

[AllowAnonymous]
[Route("[Controller]/[Action]")]
public class DocumentController : Controller
{
    private readonly IBlobContainer<Resources> _content;
    private readonly IMessageQueueSender<Resources> _queue;
    private readonly ILogger _logger;

    public DocumentController(
        IBlobContainer<Resources> content,
        IMessageQueueSender<Resources> queue,
        ILogger<DocumentController> logger
        )
    {
        _content = content;
        _queue = queue;
        _logger = logger;
    }

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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        IFormFile content,
        string? file = null,
        string? sourceContentType = null,
        bool overwrite = false
        )
    {
        _logger.LogDebug("Start Upload");
        if (ModelState.IsValid)
        {
            _logger.LogDebug("Upload Ok");
            await _content.StoreContentAsync(new()
            {
                Content = content.OpenReadStream(),
                ContentType = sourceContentType ?? content.ContentType,
                FileName = file ?? content.FileName,
            }, overwrite: overwrite);
            await _queue.SendAsync(new ResourceReference
            {
                ContentType = sourceContentType ?? content.ContentType,
                FileName = file ?? content.FileName,
                Source = nameof(Resources),
            });
            return Ok();
        }

        _logger.LogDebug("Upload BadRequest");
        return BadRequest(ModelState);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Store(
        [FromForm] ContentMetaDataReference model
        ) =>
        ModelState.IsValid && await _content.StoreContentMetaDataAsync(model) ? Ok() : BadRequest(ModelState);
}

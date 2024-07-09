using Eliassen.Documents;
using Eliassen.Documents.Models;
using Eliassen.MessageQueueing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimilaritySearchExample.Web.Models;
using System.Net;

namespace SimilaritySearchExample.Web.Controllers;

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

    /// <summary>
    /// Query all content that has been uploaded to the blob store
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpPost]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IQueryable<ContentMetaDataReference> List() => _content.QueryContent();

    /// <summary>
    /// download content from the blob store by file name
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
    /// remove an item from the blob store
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task Delete(string file) =>
        await _content.DeleteContentAsync(file);

    /// <summary>
    /// upload new content to the blob store
    /// </summary>
    /// <param name="content"></param>
    /// <param name="file"></param>
    /// <param name="sourceContentType"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
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

    /// <summary>
    /// add additional metadata to the blob store
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

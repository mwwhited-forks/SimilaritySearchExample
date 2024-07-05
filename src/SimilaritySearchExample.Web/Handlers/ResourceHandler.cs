using Eliassen.Documents;
using Eliassen.MessageQueueing;
using Eliassen.MessageQueueing.Services;
using ResourceProfiler.Web.Models;

namespace ResourceProfiler.Web.Handlers;

public class ResourceHandler : IMessageQueueHandler<Resources>
{
    private const string TargetType = "text/markdown";
    private const string TargetExtension = ".md";

    private readonly IBlobContainer<Resources> _source;
    private readonly IBlobContainer<TextResources> _target;
    private readonly IMessageQueueSender<TextResources> _queue;
    private readonly ILogger _logger;
    private readonly IDocumentConversion _converter;

    public ResourceHandler(
        IBlobContainer<Resources> source,
        IBlobContainer<TextResources> target,
        IMessageQueueSender<TextResources> queue,
        ILogger<ResourceHandler> logger,
        IDocumentConversion converter
        )
    {
        _source = source;
        _target = target;
        _queue = queue;
        _logger = logger;
        _converter = converter;
    }

    public Task HandleAsync(object message, IMessageContext context) =>
        message switch
        {
            ResourceReference r => HandleAsync(r, context),
            _ => Task.CompletedTask
        };

    public async Task HandleAsync(ResourceReference message, IMessageContext context)
    {
        _logger.LogInformation("Get: \"{filename}\"", message.FileName);
        var content = await _source.GetContentAsync(message.FileName)
            ?? throw new FileNotFoundException($"No content for \"{message.FileName}\" was found", message.FileName);

        _logger.LogTrace("Received: \"{filename}\"", message.FileName);

        _logger.LogInformation("Convert: \"{filename}\" to \"{targetType}\"", message.FileName, TargetType);
        var ms = new MemoryStream();
        await _converter.ConvertAsync(content.Content, content.ContentType, ms, TargetType);
        ms.Position = 0;
        _logger.LogTrace("Converted: \"{filename}\" to \"{targetType}\"", message.FileName, TargetType);

        var targetFile = Path.ChangeExtension(message.FileName, TargetExtension);

        _logger.LogInformation("Store: \"{filename}\"", targetFile);
        await _target.StoreContentAsync(new()
        {
            Content = ms,
            ContentType = TargetType,
            FileName = targetFile,
        }, overwrite: true);
        _logger.LogTrace("Stored: \"{filename}\"", targetFile);

        _logger.LogInformation("Notify: \"{filename}\"", targetFile);
        await _queue.SendAsync(message with
        {
            FileName = targetFile,
            Source = nameof(TextResources),
            ContentType = TargetType,
        }, context.OriginMessageId);
        _logger.LogTrace("Notified: \"{filename}\"", targetFile);
    }
}

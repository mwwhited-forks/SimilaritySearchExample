using Eliassen.MessageQueueing.Services;
using Eliassen.System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SimilaritySearchExample.Persistence;

namespace SimilaritySearchExample.Web.Extensions.MessageQueueing.Services;

public class ApplicationMessageQueueProvider : IMessageReceiverProvider, IMessageSenderProvider
{
    private IMessageHandlerProvider? _handlerProvider;
    private readonly IJsonSerializer _serializer;
    private readonly ResourceProfilerContext _db;
    private readonly ILogger<ApplicationMessageQueueProvider> _logger;

    public ApplicationMessageQueueProvider(
        IJsonSerializer serializer,
        ResourceProfilerContext db,
        ILogger<ApplicationMessageQueueProvider> logger
        )
    {
        _serializer = serializer;
        _db = db;
        _logger = logger;
    }

    public IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider)
    {
        _handlerProvider = handlerProvider;
        return this;
    }

    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        var queueName = context.Config["QueueName"];
        var wrapped = new WrappedQueueMessage()
        {
            ContentType = "application/json;",
            PayloadType = message.GetType().AssemblyQualifiedName ?? throw new NotSupportedException(),
            CorrelationId = context.CorrelationId ?? "",
            Payload = message,
            Properties = context.Headers,
        };

        var serialized = _serializer.Serialize(wrapped);

        var entity = _db.MessageQueue.Add(new MessageQueue
        {
            MessageType = context.MessageType,
            ChannelType = context.ChannelType,
            CorrelationId = context.CorrelationId,
            SentAt = context.SentAt,
            SentFrom = context.SentFrom,
            SentBy = context.SentBy,
            SentId = context.SentId,
            Content = serialized,
            QueueName = queueName,
        });
        await _db.SaveChangesAsync();

        return entity.Entity.Id.ToString();
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var newCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken).Token;

        while (!newCancellationToken.IsCancellationRequested)
        {
            //TODO: is it possible to create a read lock?
            var queueName = _handlerProvider?.Config["QueueName"];

            var message = await _db.MessageQueue.Where(i => i.QueueName == queueName)
                                   .OrderBy(i => i.Id)
                                   .FirstOrDefaultAsync(newCancellationToken);

            if (message == null)
            {
                _logger.LogInformation($"Nothing Received waiting");
                await Task.Delay(10000, cancellationToken);//TODO: move to config
                continue;
            }

            var messageId = message.Id.ToString();

            var deserialized = _serializer.Deserialize<WrappedQueueMessage>(message.Content)
                ?? throw new NotSupportedException($"No payload found");

            if (_handlerProvider != null)
                await _handlerProvider.HandleAsync(deserialized, messageId);

            _logger.LogInformation($"Dequeue: {{{nameof(messageId)}}}", messageId);

            _db.MessageQueue.Remove(message);
            await _db.SaveChangesAsync(newCancellationToken);
        }
    }
}

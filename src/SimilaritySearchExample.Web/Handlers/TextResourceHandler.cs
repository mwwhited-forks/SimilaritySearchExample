﻿using Eliassen.AI;
using Eliassen.Documents;
using Eliassen.MessageQueueing;
using Eliassen.MessageQueueing.Services;
using Eliassen.Ollama;
using Microsoft.Extensions.Options;
using SimilaritySearchExample.Web.Models;

namespace SimilaritySearchExample.Web.Handlers;

public class TextResourceHandler : IMessageQueueHandler<TextResources>
{
    private readonly IBlobContainer<TextResources> _source;
    private readonly IBlobContainer<Summaries> _target;
    private readonly IMessageQueueSender<Indexer> _queue;
    private readonly IMessageCompletion _languageModel;
    private readonly ILogger _logger;
    private readonly OllamaApiClientOptions _config;

    public TextResourceHandler(
        IBlobContainer<TextResources> source,
        IBlobContainer<Summaries> target,
        IMessageQueueSender<Indexer> queue,
        [FromKeyedServices("OLLAMA")] IMessageCompletion languageModel,
        ILogger<TextResourceHandler> logger,
        IOptions<OllamaApiClientOptions> options
        )
    {
        _source = source;
        _target = target;
        _queue = queue;
        _languageModel = languageModel;
        _logger = logger;
        _config = options.Value;
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
        var content = await _source.GetContentAsync(message.FileName);

        if (content == null)
        {
            _logger.LogWarning("No convertible content for {Source}: {FileName}", message.Source, message.FileName);
            return;
        }

        _logger.LogTrace("Received: \"{filename}\"", message.FileName);

        _logger.LogInformation("Index (text): \"{filename}\"", message.FileName);
        await _queue.SendAsync(message, context.OriginMessageId);

        var reader = new StreamReader(content.Content);
        var prompt = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(prompt))
        {
            _logger.LogWarning("No content for {messageId}: {file}", context.OriginMessageId, message.FileName);
            return;
        }

        var systemPrompt = @"Summerize the provided content";
        var summary = await _languageModel.GetCompletionAsync(new()
        {
            Model = _config.DefaultModel ?? "phi",
            Prompt = prompt,
            System = systemPrompt,
        });

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms, leaveOpen: true) { AutoFlush = true, };
        await writer.WriteAsync(summary.Response);
        ms.Position = 0;

        await _target.StoreContentAsync(content with
        {
            Content = ms,
        }, overwrite: true);

        _logger.LogInformation("Index (summary): \"{filename}\"", message.FileName);
        await _queue.SendAsync(message with { Source = nameof(Summaries) }, context.OriginMessageId);
    }
}

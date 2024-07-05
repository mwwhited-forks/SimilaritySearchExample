namespace ResourceProjectDatabase;

public class MessageQueue
{
    public int Id { get; set; }
    public string? MessageType { get; set; }
    public string? ChannelType { get; set; }
    public string? CorrelationId { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? SentFrom { get; set; }
    public string? SentBy { get; set; }
    public string? SentId { get; set; }
    public required string Content { get; set; }
    public string? QueueName { get; set; }
}

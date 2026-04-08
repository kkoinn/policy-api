using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;

public sealed class ServiceBusPublisher(
    IOptions<ServiceBusOptions>  options,
    ILogger<ServiceBusPublisher> logger
) : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusOptions _options = options.Value;
    private readonly ServiceBusSender _sender = new ServiceBusClient(
        options.Value.ConnectionString
    ).CreateSender(options.Value.TopicName);

    public async Task PublishAsync<T>(T eventData, string eventType, CancellationToken ct = default)
    {
        var message = new ServiceBusMessage(JsonSerializer.Serialize(eventData))
        {
            ContentType = "application/json",
            MessageId   = Guid.NewGuid().ToString(),
            Subject     = eventType
        };
        message.ApplicationProperties.Add("EventType", eventType);
        message.ApplicationProperties.Add("Source",    "PolicyApi");
        await _sender.SendMessageAsync(message, ct);
        logger.LogInformation("Message sent to topic: {TopicName} | MessageId: {MessageId} | EventType: {EventType}",
            _options.TopicName, message.MessageId, eventType);
    }

    public async ValueTask DisposeAsync() => await _sender.DisposeAsync();
}

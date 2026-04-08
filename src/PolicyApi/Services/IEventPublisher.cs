public interface IEventPublisher
{
    Task PublishAsync<T>(T eventData, string eventType, CancellationToken ct = default);
}

public class FakeEventPublisher : IEventPublisher
{
    public List<(object Event, string EventType)> PublishedEvents { get; } = [];

    public Task PublishAsync<T>(T eventData, string eventType, CancellationToken ct = default)
    {
        PublishedEvents.Add((eventData!, eventType));
        return Task.CompletedTask;
    }
}

namespace MrRabbit.ReliableEvents.Services;

internal class OutboxTaskFactory : IOutboxTaskFactory
{
    private readonly ISerializer _eventSerializer;

    public OutboxTaskFactory(ISerializer serializer)
    {
        _eventSerializer = serializer;
    }

    public IEnumerable<OutboxTask> Create(IEnumerable<HandlerMetadata> handlersMetadata, object @event, string? eventId, DateTime occurredDate)
    {
        var serializedEvent = _eventSerializer.Serialize(@event);
        var eventType = @event.GetType();
        return handlersMetadata.Select(handlerMetadata => new OutboxTask
        {
            Id = Guid.NewGuid(),
            QueueName = handlerMetadata.Queue.Name,
            EventId = eventId,
            HandlerFullName = handlerMetadata.Type.FullName!,
            HandlerAssemblyName = handlerMetadata.Type.Assembly.GetName().Name!,
            EventFullName = eventType.FullName!,
            EventAssemblyName = eventType.Assembly.GetName().Name!,
            EventData = serializedEvent,
            OccurredDate = occurredDate
        });
    }
}

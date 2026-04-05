namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IOutboxTaskFactory
{
    IEnumerable<OutboxTask> Create(IEnumerable<HandlerMetadata> handlersMetadata, object @event, Guid? eventId, DateTime occurredDate);
}

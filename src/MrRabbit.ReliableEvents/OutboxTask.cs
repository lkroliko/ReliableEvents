namespace MrRabbit.ReliableEvents;

public sealed class OutboxTask
{
    public required Guid Id { get; init; }
    public required string QueueName { get; init; }
    public required Guid? EventId { get; init; }
    public required string HandlerFullName { get; init; }
    public required string HandlerAssemblyName { get; init; }
    public required string EventFullName { get; init; }
    public required string EventAssemblyName { get; init; }
    public required string EventData { get; init; }
    public required DateTime OccurredDate { get; init; }
    public bool IsDispatched { get; private set; }

    public void MarkAsDispatched()
    {
        IsDispatched = true;
    }
}

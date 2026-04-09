namespace MrRabbit.ReliableEvents;

public class OutboxDispatchQueueErrorContext
{
    public required OutboxQueue Queue { get; init; }
    public required Exception Exception { get; init; }
}
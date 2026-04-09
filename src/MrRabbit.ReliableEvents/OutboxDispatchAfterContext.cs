namespace MrRabbit.ReliableEvents;

public class OutboxDispatchedQueueContext
{
    public required OutboxQueue Queue { get; init; }
    public required int DispatchedTasksCount { get; init; }
}

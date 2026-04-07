namespace MrRabbit.ReliableEvents;

public class OutboxDispatchedContext
{
    public required OutboxQueue Queue { get; init; }
    public required int DispatchedTasksCount { get; init; }
}

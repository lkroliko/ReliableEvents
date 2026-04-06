namespace MrRabbit.ReliableEvents;

public class OutboxDispatchAfterContext
{
    public required OutboxQueue Queue { get; init; }
    public required int DispatchedTasksCount { get; init; }
}

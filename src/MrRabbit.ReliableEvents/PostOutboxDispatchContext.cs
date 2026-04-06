namespace MrRabbit.ReliableEvents;

public class PostOutboxDispatchContext
{
    public required OutboxQueue Queue { get; init; }
    public required int DispatchedTasksCount { get; init; }
}
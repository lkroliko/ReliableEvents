namespace MrRabbit.ReliableEvents;

public class AfterOutboxDispatchContext
{
    public required OutboxQueue Queue { get; init; }
    public required int DispatchedTasksCount { get; init; }
}
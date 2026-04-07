namespace MrRabbit.ReliableEvents;

public class OutboxDispatchErrorContext
{
    public required OutboxQueue Queue { get; init; }
    public required Exception Exception { get; init; }
}
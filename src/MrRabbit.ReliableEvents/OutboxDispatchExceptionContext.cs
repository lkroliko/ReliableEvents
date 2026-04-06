namespace MrRabbit.ReliableEvents;

public class OutboxDispatchExceptionContext
{
    public required OutboxQueue Queue { get; init; }
    public required Exception Exception { get; init; }

}
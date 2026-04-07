namespace MrRabbit.ReliableEvents;

public interface IOutboxDispatchedHandler
{
    Task HandleAsync(OutboxDispatchedContext context);
}

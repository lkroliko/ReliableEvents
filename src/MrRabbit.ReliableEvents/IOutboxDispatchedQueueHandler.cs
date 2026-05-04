namespace MrRabbit.ReliableEvents;

public interface IOutboxDispatchedQueueHandler
{
    Task HandleAsync(OutboxDispatchedQueueContext context);
}

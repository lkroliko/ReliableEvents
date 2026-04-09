namespace MrRabbit.ReliableEvents;

public interface IOutboxDispatchQueueErrorHandler
{
    Task HandleAsync(OutboxDispatchQueueErrorContext context);
}
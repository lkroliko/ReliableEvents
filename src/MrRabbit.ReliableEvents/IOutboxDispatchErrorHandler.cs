namespace MrRabbit.ReliableEvents;

public interface IOutboxDispatchErrorHandler
{
    Task HandleAsync(OutboxDispatchErrorContext context);
}
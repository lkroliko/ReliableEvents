namespace MrRabbit.ReliableEvents;

public interface IOutboxDispatchErrorHandler //TODO queue dispatch
{
    Task HandleAsync(OutboxDispatchErrorContext context);
}
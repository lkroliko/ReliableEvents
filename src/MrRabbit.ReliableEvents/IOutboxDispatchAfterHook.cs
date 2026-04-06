namespace MrRabbit.ReliableEvents;

public interface IOutboxDispatchAfterHook
{
    Task AfterDispatchAsync(OutboxDispatchAfterContext context);
}

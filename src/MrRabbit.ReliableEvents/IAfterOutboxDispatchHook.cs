namespace MrRabbit.ReliableEvents;

public interface IAfterOutboxDispatchHook
{
    Task AfterDispatchAsync(AfterOutboxDispatchContext context);
}

namespace MrRabbit.ReliableEvents;

public interface IOutboxDispatchExceptionHook
{
    Task OnDispatchExceptionAsync(OutboxDispatchExceptionContext context);
}
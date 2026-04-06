namespace MrRabbit.ReliableEvents;

public interface IOutboxDispatchPostProcessor
{
    Task ProcessAsync(PostOutboxDispatchContext context);
}

namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestrOutboxDispatchedHandler : IOutboxDispatchedQueueHandler
{
    public virtual Task HandleAsync(OutboxDispatchedQueueContext context) => Task.CompletedTask;
}
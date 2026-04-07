namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestrOutboxDispatchedHandler : IOutboxDispatchedHandler
{
    public virtual Task HandleAsync(OutboxDispatchedContext context) => Task.CompletedTask;
}
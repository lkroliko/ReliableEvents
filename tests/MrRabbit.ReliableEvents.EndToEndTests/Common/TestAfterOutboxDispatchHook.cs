namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestAfterOutboxDispatchHook : IOutboxDispatchAfterHook
{
    public virtual Task AfterDispatchAsync(OutboxDispatchAfterContext context) => Task.CompletedTask;
}
namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestAfterOutboxDispatchHook : IAfterOutboxDispatchHook
{
    public virtual Task AfterDispatchAsync(AfterOutboxDispatchContext context) => Task.CompletedTask;
}
namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IOutboxDispatchHookInvoker
{
    Task InvokeAfterHooksAsync(OutboxQueue queue, int dispatchedTasksCount);
}

internal class OutboxDispatchHookInvoker : IOutboxDispatchHookInvoker
{
    private readonly IEnumerable<IAfterOutboxDispatchHook> _postHooks;
    public OutboxDispatchHookInvoker(IEnumerable<IAfterOutboxDispatchHook> postHooks)
    {
        _postHooks = postHooks;
    }

    public async Task InvokeAfterHooksAsync(OutboxQueue queue, int dispatchedTasksCount)
    {
        var context = new AfterOutboxDispatchContext()
        {
            Queue = queue,
            DispatchedTasksCount = dispatchedTasksCount,
        };
        try
        {
            foreach (var postHook in _postHooks)
                await postHook.AfterDispatchAsync(context);
        }
        catch (Exception ex)
        {
            throw new ReliableEventsException("An error occurred while running 'IAfterOutboxDispatchHook'.", ex);
        }
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class OutboxDispatchHookInvoker : IOutboxDispatchHookInvoker
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxDispatchHookInvoker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAfterHooksAsync(OutboxQueue queue, int dispatchedTasksCount)
    {
        var context = new OutboxDispatchAfterContext()
        {
            Queue = queue,
            DispatchedTasksCount = dispatchedTasksCount,
        };
        var hooks = _serviceProvider.GetServices<IOutboxDispatchAfterHook>();
        try
        {
            foreach (var hook in hooks)
                await hook.AfterDispatchAsync(context);
        }
        catch (Exception ex)
        {
            throw new ReliableEventsException($"An error occurred while running '{nameof(IOutboxDispatchAfterHook)}'.", ex);
        }
    }

    public async Task InvokeExceptionHooksAsync(DispatchResult result)
    {
        var context = new OutboxDispatchExceptionContext()
        {
            Queue = result.Queue,
            Exception = result.Exception!,
        };
        var hooks = _serviceProvider.GetServices<IOutboxDispatchExceptionHook>();
        try
        {
            foreach (var hook in hooks)
                await hook.OnDispatchExceptionAsync(context);
        }
        catch (Exception ex)
        {
            throw new ReliableEventsException($"An exception occurred while handling an '{nameof(IOutboxDispatchExceptionHook)}'.", ex);
        }
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class OutboxHandlerInvoker : IOutboxHandlerInvoker
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxHandlerInvoker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeDispatchedQueueHandlerAsync(OutboxQueue queue, int dispatchedTasksCount)
    {
        var context = new OutboxDispatchedQueueContext()
        {
            Queue = queue,
            DispatchedTasksCount = dispatchedTasksCount,
        };
        var hooks = _serviceProvider.GetServices<IOutboxDispatchedQueueHandler>();
        try
        {
            foreach (var hook in hooks)
                await hook.HandleAsync(context);
        }
        catch (Exception ex)
        {
            throw new ReliableEventsException($"An error occurred while running '{nameof(IOutboxDispatchedQueueHandler)}'.", ex);
        }
    }

    public async Task InvokeDispatchQueueErrorHandlerAsync(DispatchResult result)
    {
        var context = new OutboxDispatchQueueErrorContext()
        {
            Queue = result.Queue,
            Exception = result.Exception!,
        };
        var hooks = _serviceProvider.GetServices<IOutboxDispatchQueueErrorHandler>();
        try
        {
            foreach (var hook in hooks)
                await hook.HandleAsync(context);
        }
        catch (Exception ex)
        {
            throw new ReliableEventsException($"An exception occurred while handling an '{nameof(IOutboxDispatchQueueErrorHandler)}'.", ex);
        }
    }
}
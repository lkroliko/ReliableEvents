using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class OutboxHandlerInvoker : IOutboxHandlerInvoker
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxHandlerInvoker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeDispatchedHandlerAsync(OutboxQueue queue, int dispatchedTasksCount)
    {
        var context = new OutboxDispatchedContext()
        {
            Queue = queue,
            DispatchedTasksCount = dispatchedTasksCount,
        };
        var hooks = _serviceProvider.GetServices<IOutboxDispatchedHandler>();
        try
        {
            foreach (var hook in hooks)
                await hook.HandleAsync(context);
        }
        catch (Exception ex)
        {
            throw new ReliableEventsException($"An error occurred while running '{nameof(IOutboxDispatchedHandler)}'.", ex);
        }
    }

    public async Task InvokeDispatchErrorHandlerAsync(DispatchResult result)
    {
        var context = new OutboxDispatchErrorContext()
        {
            Queue = result.Queue,
            Exception = result.Exception!,
        };
        var hooks = _serviceProvider.GetServices<IOutboxDispatchErrorHandler>();
        try
        {
            foreach (var hook in hooks)
                await hook.HandleAsync(context);
        }
        catch (Exception ex)
        {
            throw new ReliableEventsException($"An exception occurred while handling an '{nameof(IOutboxDispatchErrorHandler)}'.", ex);
        }
    }
}
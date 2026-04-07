using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IEnumerable<object> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            if (@event is null)
                continue;
            var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());

            var handlers = _serviceProvider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                var handleMethod = handlerType.GetMethod(nameof(IEventHandler<>.HandleAsync));
                var task = (Task)handleMethod!.Invoke(handler, [@event, cancellationToken])!;
                await task;
            }
        }
    }
}

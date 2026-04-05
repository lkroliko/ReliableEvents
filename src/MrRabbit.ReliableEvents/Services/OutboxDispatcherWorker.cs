using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class OutboxDispatcherWorker : IOutboxDispatcherWorker
{
    private readonly ISerializer _serializer;
    private readonly IServiceProvider _serviceProvider;

    public OutboxDispatcherWorker(ISerializer serializer, IServiceProvider serviceProvider)
    {
        _serializer = serializer;
        _serviceProvider = serviceProvider;
    }

    public async Task<DispatchResult> DispatchAsync(OutboxQueue queue, OutboxTask outboxTask, CancellationToken cancellationToken)
    {
        var integrationEvent = GetEvent();
        var handlerType = GetHandlerType();
        var handler = GetHandler();
        var methodInfo = handlerType.GetMethod(nameof(IOutboxEventHandler<>.HandleAsync));
        try
        {
            await (Task)methodInfo.Invoke(handler, new[] { integrationEvent, cancellationToken })!;
        }
        catch (Exception ex)
        {
            return DispatchResult.Fail(queue, ex);
        }
        return DispatchResult.Ok(queue);

        object GetEvent()
        {
            var typeName = $"{outboxTask.EventFullName}, {outboxTask.EventAssemblyName}";
            var eventType = Type.GetType(typeName) ?? throw new ReliableEventException($"Event type '{typeName}' not found.");
            return _serializer.Deserialize(outboxTask.EventData, eventType) ?? throw new ReliableEventException($"Failed to deserialize event data for type '{typeName}'.");
        }

        Type GetHandlerType()
        {
            var typeName = $"{outboxTask.HandlerFullName}, {outboxTask.HandlerAssemblyName}";
            return Type.GetType(typeName) ?? throw new ReliableEventException($"Handler type '{typeName}' not found.");
        }

        object GetHandler()
        {
            var typeName = $"{outboxTask.HandlerFullName}, {outboxTask.HandlerAssemblyName}";
            var handlerType = Type.GetType(typeName) ?? throw new ReliableEventException($"Handler type '{typeName}' not found.");
            return _serviceProvider.GetRequiredService(handlerType);
        }
    }
}

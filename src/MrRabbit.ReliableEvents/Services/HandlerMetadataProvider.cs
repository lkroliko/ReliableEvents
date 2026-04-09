using MrRabbit.ReliableEvents.Attributes;
using System.Reflection;

namespace MrRabbit.ReliableEvents.Services;

internal sealed class HandlerMetadataProvider<TDbContext> : IHandlerMetadataProvider<TDbContext> where TDbContext : DbContext
{
    private readonly Dictionary<Type, List<HandlerMetadata>> _handlerMetadatasForEvent = [];

    public HandlerMetadataProvider(IEnumerable<Type> outboxEventHandlerTypes)
    {
        foreach (var type in outboxEventHandlerTypes)
        {
            var queueAttribute = type.GetCustomAttribute<EventHandlerQueueAttribute>();
            if (queueAttribute is null)
                throw new InvalidOperationException($"Integration event handler {type.FullName} must have '{nameof(EventHandlerQueueAttribute)}'.");

            var integrationEventType = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IOutboxEventHandler<>))?.GetGenericArguments().FirstOrDefault();
            var handlerMetadata = new HandlerMetadata() { Type = type, Queue = queueAttribute.Queue };
            if (_handlerMetadatasForEvent.TryGetValue(integrationEventType!, out var handlerMetadatasForIntegrationEvent))
                handlerMetadatasForIntegrationEvent.Add(handlerMetadata);
            else
                _handlerMetadatasForEvent.Add(integrationEventType!, new() { handlerMetadata });
        }
    }

    public IEnumerable<HandlerMetadata> GetHandlersMetadata(object @event) =>
        _handlerMetadatasForEvent.TryGetValue(@event.GetType(), out var handlerMetadatas) ? handlerMetadatas : Enumerable.Empty<HandlerMetadata>();
}

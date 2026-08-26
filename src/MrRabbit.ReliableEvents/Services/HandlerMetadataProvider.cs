using MrRabbit.ReliableEvents.Attributes;
using System.Reflection;

namespace MrRabbit.ReliableEvents.Services;

internal sealed class HandlerMetadataProvider<TDbContext> : IHandlerMetadataProvider<TDbContext> where TDbContext : DbContext
{
    private readonly Dictionary<Type, List<HandlerMetadata>> _handlerMetadatasForEvent = [];

    public HandlerMetadataProvider(IEnumerable<Type> outboxEventHandlerTypes)
    {
        foreach (var type in outboxEventHandlerTypes.Distinct())
        {
            var queueAttribute = type.GetCustomAttribute<EventHandlerQueueAttribute>();
            if (queueAttribute is null)
                throw new InvalidOperationException($"Integration event handler {type.FullName} must have '{nameof(EventHandlerQueueAttribute)}'.");

            var integrationEventTypes = type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IOutboxEventHandler<>)).Select(x => x.GetGenericArguments().First()).ToArray();
            var handlerMetadata = new HandlerMetadata() { Type = type, Queue = queueAttribute.Queue };
            foreach (var integrationEventType in integrationEventTypes)
            {
                if (_handlerMetadatasForEvent.TryGetValue(integrationEventType, out var handlerMetadatasForIntegrationEvent))
                    handlerMetadatasForIntegrationEvent.Add(handlerMetadata);
                else
                    _handlerMetadatasForEvent.Add(integrationEventType, new() { handlerMetadata });
            }
        }
    }

    public IEnumerable<HandlerMetadata> GetHandlersMetadata(object @event) =>
        _handlerMetadatasForEvent.TryGetValue(@event.GetType(), out var handlerMetadatas) ? handlerMetadatas : Enumerable.Empty<HandlerMetadata>();
}

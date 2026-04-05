using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MrRabbit.ReliableEvents.Builders;

public class ReliableEventsOptionsBuilder
{
    private readonly IServiceCollection _services;

    internal List<Type> OutboxHandlerTypes { get; } = [];

    internal ReliableEventsOptionsBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public ReliableEventsOptionsBuilder AddEventHandlers(params Assembly[] assemblies)
    {
        foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
        {
            if (type is { IsAbstract: false, IsInterface: false })
            {
                foreach (var interfaceType in type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
                {
                    _services.AddScoped(interfaceType, type);
                }
            }
        }

        return this;
    }

    public ReliableEventsOptionsBuilder AddEventHandler(Type type)
    {
        var interfaceType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        if (interfaceType is null)
            throw new ArgumentException($"Type '{type.Name}' not implementing IEventHandler<>.");
        _services.AddScoped(interfaceType, type);

        return this;
    }

    public ReliableEventsOptionsBuilder AddOutboxEventHandlers(params Assembly[] assemblies)
    {
        foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
        {
            if (type is { IsAbstract: false, IsInterface: false })
            {
                foreach (var interfaceType in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IOutboxEventHandler<>)))
                {
                    _services.AddScoped(type);
                    OutboxHandlerTypes.Add(type);
                }
            }
        }

        return this;
    }

    public ReliableEventsOptionsBuilder AddOutboxEventHandler(Type type)
    {
        var interfaceType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IOutboxEventHandler<>));
        if (interfaceType is null)
            throw new ArgumentException($"Type '{type.Name}' not implementing IOutboxEventHandler<>.");
        _services.AddScoped(type);
        OutboxHandlerTypes.Add(type);

        return this;
    }
}

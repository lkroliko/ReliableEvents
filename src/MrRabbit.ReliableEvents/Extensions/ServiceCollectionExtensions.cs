using MrRabbit.ReliableEvents;
using MrRabbit.ReliableEvents.Builders;
using MrRabbit.ReliableEvents.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddReliableEvents<TDbContext>(this IServiceCollection services, Action<ReliableEventsOptionsBuilder> optionsAction) where TDbContext : DbContext
    {
        var optionsBuilder = new ReliableEventsOptionsBuilder(services);
        optionsAction.Invoke(optionsBuilder);

        services.AddSingleton<IDispatcher, Dispatcher>();
        services.AddScoped(typeof(IReliableEvents<TDbContext>), typeof(ReliableEvents<TDbContext>));
        services.AddSingleton<ISerializer, JsonSerializer>();
        services.AddSingleton(typeof(IHandlerMetadataProvider<TDbContext>), _ => new HandlerMetadataProvider<TDbContext>(optionsBuilder.OutboxHandlerTypes));
        services.AddSingleton<IOutboxDispatcherWorker, OutboxDispatcherWorker>();
        services.AddScoped<IOutboxDispatcher<TDbContext>, OutboxDispatcher<TDbContext>>();
        services.AddScoped<IQueueOutboxDispatcher<TDbContext>, QueueOutboxDispatcher<TDbContext>>();
        services.AddScoped<IOutboxStore<TDbContext>, OutboxStore<TDbContext>>();
        services.AddSingleton<IOutboxTaskFactory, OutboxTaskFactory>();
        services.AddScoped<IUnitOfWork<TDbContext>, UnitOfWork<TDbContext>>();
        services.AddSingleton<IOutboxQueueSemaphoreProvider<TDbContext>, OutboxQueueSemaphoreProvider<TDbContext>>();
    }
}

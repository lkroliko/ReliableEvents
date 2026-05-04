using MrRabbit.ReliableEvents;
using MrRabbit.ReliableEvents.Builders;
using MrRabbit.ReliableEvents.Services;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ServiceCollectionExtensions
{
    public static void AddReliableEvents<TDbContext>(this IServiceCollection services, Action<ReliableEventsOptionsBuilder> optionsAction) where TDbContext : DbContext
    {
        var optionsBuilder = new ReliableEventsOptionsBuilder(services);
        optionsAction.Invoke(optionsBuilder);

        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IReliableEvents<TDbContext>, ReliableEvents<TDbContext>>();
        services.AddSingleton<ISerializer, JsonSerializer>();
        services.AddSingleton(typeof(IHandlerMetadataProvider<TDbContext>), _ => new HandlerMetadataProvider<TDbContext>(optionsBuilder.OutboxHandlerTypes));
        services.AddScoped<IOutboxDispatcherWorker, OutboxDispatcherWorker>();
        services.AddSingleton<IOutboxDispatcher<TDbContext>, OutboxDispatcher<TDbContext>>();
        services.AddScoped<IQueueOutboxDispatcher<TDbContext>, QueueOutboxDispatcher<TDbContext>>();
        services.AddScoped<IOutboxStore<TDbContext>, OutboxStore<TDbContext>>();
        services.AddSingleton<IOutboxTaskFactory, OutboxTaskFactory>();
        services.AddScoped<IUnitOfWork<TDbContext>, UnitOfWork<TDbContext>>();
        services.AddSingleton<IOutboxQueueSemaphoreProvider<TDbContext>, OutboxQueueSemaphoreProvider<TDbContext>>();
        services.AddScoped<IOutboxHandlerInvoker, OutboxHandlerInvoker>();
        services.AddScoped<IOutboxStoreStatistics<TDbContext>, OutboxStoreStatistics<TDbContext>>();
    }
}

namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IQueueOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    Task DispatchAsync(Queue queue, CancellationToken cancellationToken = default);
}
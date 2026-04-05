namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IQueueOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    Task<DispatchResult> DispatchAsync(OutboxQueue queue, CancellationToken cancellationToken = default);
}
namespace MrRabbit.ReliableEvents.Interfaces;

public interface IOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    Task<DispatchResult> DispatchAsync(OutboxQueue queue, CancellationToken cancellationToken);
    Task<DispatchResult[]> DispatchAsync(IEnumerable<OutboxQueue> queues, CancellationToken cancellationToken = default);
    Task<DispatchResult[]> DispatchAsync(CancellationToken cancellationToken);
}

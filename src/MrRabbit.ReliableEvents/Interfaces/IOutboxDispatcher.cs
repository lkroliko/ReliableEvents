namespace MrRabbit.ReliableEvents.Interfaces;

public interface IOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    Task<DispatchResult[]> DispatchAsync(IEnumerable<OutboxQueue> queues);
}

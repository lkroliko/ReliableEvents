namespace MrRabbit.ReliableEvents.Interfaces;

public interface IOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    Task DispatchAsync(IEnumerable<Queue> queues);
}

using System.Collections.Concurrent;

namespace MrRabbit.ReliableEvents.Services;

internal class OutboxQueueSemaphoreProvider<TDbContext> : IOutboxQueueSemaphoreProvider<TDbContext> where TDbContext : DbContext
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

    public SemaphoreSlim Get(OutboxQueue queue)
    {
        return _semaphores.GetOrAdd(queue.Name, _ => new SemaphoreSlim(1, 1));
    }
}

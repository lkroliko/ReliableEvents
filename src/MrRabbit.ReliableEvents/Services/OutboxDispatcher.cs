namespace MrRabbit.ReliableEvents.Services;

internal class OutboxDispatcher<TDbContext> : IOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    private readonly IQueueOutboxDispatcher<TDbContext> _outboxDispatcherWorker;

    public OutboxDispatcher(IQueueOutboxDispatcher<TDbContext> outboxDispatcherWorker)
    {
        _outboxDispatcherWorker = outboxDispatcherWorker;
    }

    public async Task<DispatchResult[]> DispatchAsync(IEnumerable<OutboxQueue> queues, CancellationToken cancellationToken)
    {
        return await Task.WhenAll(queues.Select(queue => _outboxDispatcherWorker.DispatchAsync(queue, cancellationToken)));
    }
}

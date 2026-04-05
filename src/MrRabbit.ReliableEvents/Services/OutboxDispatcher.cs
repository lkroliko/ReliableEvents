namespace MrRabbit.ReliableEvents.Services;

internal class OutboxDispatcher<TDbContext> : IOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    private readonly IQueueOutboxDispatcher<TDbContext> _outboxDispatcherWorker;

    public OutboxDispatcher(IQueueOutboxDispatcher<TDbContext> outboxDispatcherWorker)
    {
        _outboxDispatcherWorker = outboxDispatcherWorker;
    }

    public async Task DispatchAsync(IEnumerable<Queue> queues)
    {
        await Task.WhenAll(queues.Select(queue => _outboxDispatcherWorker.DispatchAsync(queue)));
        //tODO brak zabezpieczenia przed wielokrotnym wywołaniem tego samego zadania, np. poprzez dodanie do bazy danych rekordu z informacją o tym, że zadanie jest już wykonywane
    }
}

namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IOutboxDispatcherWorker
{
    public Task<DispatchResult> DispatchAsync(OutboxQueue queue, OutboxTask outboxTask, CancellationToken cancellationToken);
}

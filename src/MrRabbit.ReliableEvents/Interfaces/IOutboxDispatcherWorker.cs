namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IOutboxDispatcherWorker
{
    public Task DispatchAsync(OutboxTask outboxTask, CancellationToken cancellationToken);
}

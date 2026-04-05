namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IRepository
{
    Task<OutboxTask?> GetOldestOutboxTaskAsync(OutboxQueue queue);

    void AddRange(IEnumerable<OutboxTask> outboxTasks);
    void Remove(OutboxTask outboxTask);
}
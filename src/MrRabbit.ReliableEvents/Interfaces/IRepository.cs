namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IRepository
{
    Task<OutboxTask?> GetOldestOutboxTaskAsync(Queue queue);

    void AddRange(IEnumerable<OutboxTask> outboxTasks);
    void Remove(OutboxTask outboxTask);
}
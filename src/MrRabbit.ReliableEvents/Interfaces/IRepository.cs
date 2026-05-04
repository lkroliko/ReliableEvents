namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IRepository
{
    Task<OutboxTask?> GetOldestOutboxTaskAsync(OutboxQueue queue);
    Task<bool> AnyAsync(string eventId, CancellationToken cancellationToken = default);
    Task<List<string>> GetQueuesToDispatchAsync();
    Task<List<OutboxTask>> GetNotDispatchedOutboxTasksAsync();

    void AddRange(IEnumerable<OutboxTask> outboxTasks);
    void Remove(OutboxTask outboxTask);
}
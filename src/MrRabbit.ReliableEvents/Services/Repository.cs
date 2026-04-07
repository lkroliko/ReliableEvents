namespace MrRabbit.ReliableEvents.Services;

internal sealed class Repository : IRepository
{
    private readonly DbContext _dbContext;
    public Repository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddRange(IEnumerable<OutboxTask> outboxTasks) =>
        _dbContext.AddRange(outboxTasks);

    public async Task<OutboxTask?> GetOldestOutboxTaskAsync(OutboxQueue queue) =>
        await _dbContext.Set<OutboxTask>()
            .Where(x => x.IsDispatched == false && x.QueueName == queue.Name)
            .OrderBy(x => x.OccurredDate)
            .FirstOrDefaultAsync();
    public void Remove(OutboxTask outboxTask) =>
        _dbContext.Remove(outboxTask);

    public async Task<bool> AnyAsync(string eventId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<OutboxTask>().AnyAsync(x => x.EventId == eventId, cancellationToken);

    public async Task<List<string>> GetQueuesAsync() =>
        await _dbContext.Set<OutboxTask>().Select(t => t.QueueName).Distinct().ToListAsync();
}
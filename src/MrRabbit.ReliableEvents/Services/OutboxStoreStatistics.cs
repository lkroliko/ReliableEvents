namespace MrRabbit.ReliableEvents.Services;

internal class OutboxStoreStatistics<TDbContext> : IOutboxStoreStatistics<TDbContext> where TDbContext : DbContext
{
    private readonly IUnitOfWork<TDbContext> _unitOfWork;

    public OutboxStoreStatistics(IUnitOfWork<TDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WaitingTaskStatistics> GetWaitingTaskStatisticsAsync()
    {
        var outboxTasks = await _unitOfWork.Repository.GetNotDispatchedOutboxTasksAsync();

        var queues = outboxTasks
            .GroupBy(x => x.QueueName)
            .Select(g => new WaitingTaskStatistics.QueueWaitingTaskStatistics
            {
                Queue = new OutboxQueue(g.Key),
                WaitingTasksCount = g.Count()
            })
            .ToArray();

        return new WaitingTaskStatistics()
        {
            Queues = queues,
            WaitingTasksCount = outboxTasks.Count(),
        };
    }
}

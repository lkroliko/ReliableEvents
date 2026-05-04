namespace MrRabbit.ReliableEvents;

public interface IOutboxStoreStatistics<TDbContext> where TDbContext : DbContext
{
    Task<WaitingTaskStatistics> GetWaitingTaskStatisticsAsync();
}
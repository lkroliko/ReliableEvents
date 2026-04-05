namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IOutboxQueueSemaphoreProvider<TDbContext> where TDbContext : DbContext
{
    SemaphoreSlim Get(OutboxQueue queue);
}
namespace MrRabbit.ReliableEvents;

public interface IReliableEvents<TDbContext> where TDbContext : DbContext
{
    IDispatcher Dispatcher { get; }
    IOutboxStore<TDbContext> OutboxStore { get; }
    IOutboxDispatcher<TDbContext> OutboxDispatcher { get; }//TODO nie działa w dbcontext aby uruchomić dispatchera
}

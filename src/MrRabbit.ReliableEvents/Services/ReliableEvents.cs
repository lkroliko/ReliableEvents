namespace MrRabbit.ReliableEvents.Services;

internal class ReliableEvents<TDbContext> : IReliableEvents<TDbContext> where TDbContext : DbContext
{
    public IDispatcher Dispatcher { get; }
    public IOutboxStore<TDbContext> OutboxStore { get; }
    public IOutboxDispatcher<TDbContext> OutboxDispatcher { get; }

    public ReliableEvents(IDispatcher dispatcher, IOutboxStore<TDbContext> store, IOutboxDispatcher<TDbContext> outboxDispatcher)
    {
        Dispatcher = dispatcher;
        OutboxStore = store;
        OutboxDispatcher = outboxDispatcher;
    }
}

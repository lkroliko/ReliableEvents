namespace MrRabbit.ReliableEvents.Interfaces;

public interface IOutboxStore<TDbContext> where TDbContext : DbContext
{
    IEnumerable<OutboxQueue> AttachEvent(object @event, Guid? eventId, DateTime occurredDate);

    IEnumerable<OutboxQueue> AttachEvents<TEvent>(IEnumerable<TEvent> events, Func<TEvent, Guid?> guidFactory, Func<TEvent, DateTime> occurredDateFactory);
}

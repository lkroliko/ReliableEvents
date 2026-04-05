namespace MrRabbit.ReliableEvents.Interfaces;

public interface IOutboxStore<TDbContext> where TDbContext : DbContext
{
    IEnumerable<OutboxQueue> AttachEvent(object @event, string? eventId, DateTime occurredDate);

    IEnumerable<OutboxQueue> AttachEvents<TEvent>(IEnumerable<TEvent> events, Func<TEvent, string?> guidFactory, Func<TEvent, DateTime> occurredDateFactory);
}

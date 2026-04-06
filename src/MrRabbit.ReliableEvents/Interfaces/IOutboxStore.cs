namespace MrRabbit.ReliableEvents.Interfaces;

public interface IOutboxStore<TDbContext> where TDbContext : DbContext
{
    IEnumerable<OutboxQueue> AttachEvent(object @event, string? eventId, DateTime occurredDate);
    IEnumerable<OutboxQueue> AttachEvents<TEvent>(IEnumerable<TEvent> events, Func<TEvent, string?> eventIdFactory, Func<TEvent, DateTime> occurredDateFactory);

    Task<IEnumerable<OutboxQueue>> TryAddEventAsync(object @event, string eventId, DateTime occurredDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<OutboxQueue>> AddEventsAsync<TEvent>(IEnumerable<TEvent> events, Func<TEvent, string> eventIdFactory, Func<TEvent, DateTime> occurredDateFactory, CancellationToken cancellationToken = default) where TEvent : notnull;
}

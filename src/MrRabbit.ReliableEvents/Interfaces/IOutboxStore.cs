namespace MrRabbit.ReliableEvents.Interfaces;

public interface IOutboxStore<TDbContext> where TDbContext : DbContext
{
    IEnumerable<Queue> AttachEvent(object @event, Guid? eventId, DateTime occurredDate);

    IEnumerable<Queue> AttachEvents<TEvent>(IEnumerable<TEvent> events, Func<TEvent, Guid?> guidFactory, Func<TEvent, DateTime> occurredDateFactory);
}

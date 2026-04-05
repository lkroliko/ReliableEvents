namespace MrRabbit.ReliableEvents.Services;

internal class OutboxStore<TDbContext> : IOutboxStore<TDbContext> where TDbContext : DbContext
{
    private readonly IHandlerMetadataProvider<TDbContext> _handlerMetadataProvider;
    private readonly IOutboxTaskFactory _outboxTaskFactory;
    private readonly IUnitOfWork<TDbContext> _unitOfWork;
    private readonly List<string> _attachedEventIds = [];
    public OutboxStore(IHandlerMetadataProvider<TDbContext> handlerMetadataProvider, IOutboxTaskFactory outboxTaskFactory, IUnitOfWork<TDbContext> unitOfWork)
    {
        _handlerMetadataProvider = handlerMetadataProvider;
        _outboxTaskFactory = outboxTaskFactory;
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<OutboxQueue> AttachEvent(object @event, string? eventId, DateTime occurredDate)
    {
        ValidateEventId(eventId);
        var handlersMetadata = _handlerMetadataProvider.GetHandlersMetadata(@event);
        var outboxTasks = _outboxTaskFactory.Create(handlersMetadata, @event, eventId, occurredDate);
        _unitOfWork.Repository.AddRange(outboxTasks);

        return handlersMetadata.Select(x => x.Queue).Distinct();
    }

    public IEnumerable<OutboxQueue> AttachEvents<TEvent>(IEnumerable<TEvent> events, Func<TEvent, string?> eventIdFactory, Func<TEvent, DateTime> occurredDateFactory)
    {
        var queues = new List<OutboxQueue>();
        foreach (var @event in events)
        {
            var eventId = eventIdFactory(@event);
            ValidateEventId(eventId);
            var handlersMetadata = _handlerMetadataProvider.GetHandlersMetadata(@event);
            var outboxTasks = _outboxTaskFactory.Create(handlersMetadata, @event, eventId, occurredDateFactory(@event));
            _unitOfWork.Repository.AddRange(outboxTasks);
            queues.AddRange(handlersMetadata.Select(x => x.Queue));
        }

        return queues.Distinct();
    }

    private void ValidateEventId(string? eventId)
    {
        if (eventId is null)
            return;
        if (_attachedEventIds.Contains(eventId))
            throw new InvalidOperationException($"Event with id '{eventId}' is already attached.");
        _attachedEventIds.Add(eventId);
    }
}

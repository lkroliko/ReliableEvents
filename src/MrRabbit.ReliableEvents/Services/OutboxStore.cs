namespace MrRabbit.ReliableEvents.Services;

internal class OutboxStore<TDbContext> : IOutboxStore<TDbContext> where TDbContext : DbContext
{
    private readonly IHandlerMetadataProvider<TDbContext> _handlerMetadataProvider;
    private readonly IOutboxTaskFactory _outboxTaskFactory;
    private readonly IUnitOfWork<TDbContext> _unitOfWork;

    public OutboxStore(IHandlerMetadataProvider<TDbContext> handlerMetadataProvider, IOutboxTaskFactory outboxTaskFactory, IUnitOfWork<TDbContext> unitOfWork)
    {
        _handlerMetadataProvider = handlerMetadataProvider;
        _outboxTaskFactory = outboxTaskFactory;
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<OutboxQueue> AttachEvent(object @event, Guid? eventId, DateTime occurredDate)
    {
        var handlersMetadata = _handlerMetadataProvider.GetHandlersMetadata(@event);
        var outboxTasks = _outboxTaskFactory.Create(handlersMetadata, @event, eventId, occurredDate);
        _unitOfWork.Repository.AddRange(outboxTasks);

        return handlersMetadata.Select(x => x.Queue).Distinct();
    }

    public IEnumerable<OutboxQueue> AttachEvents<TEvent>(IEnumerable<TEvent> events, Func<TEvent, Guid?> guidFactory, Func<TEvent, DateTime> occurredDateFactory)
    {
        var queues = new List<OutboxQueue>();
        foreach (var @event in events)
        {
            var handlersMetadata = _handlerMetadataProvider.GetHandlersMetadata(@event);
            var outboxTasks = _outboxTaskFactory.Create(handlersMetadata, @event, guidFactory(@event), occurredDateFactory(@event));
            _unitOfWork.Repository.AddRange(outboxTasks);
            queues.AddRange(handlersMetadata.Select(x => x.Queue));
        }

        return queues.Distinct();
    }
}

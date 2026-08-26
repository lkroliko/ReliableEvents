namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

[EventHandlerQueue(TestConsts.Queues.Queue1.Name)]
internal class OutboxABEventHandlerForQueue1 : OutboxEventHandlerBase, IOutboxEventHandler<OutboxAEvent>, IOutboxEventHandler<OutboxBEvent>
{
    public virtual Task HandleAsync(OutboxAEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task HandleAsync(OutboxBEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

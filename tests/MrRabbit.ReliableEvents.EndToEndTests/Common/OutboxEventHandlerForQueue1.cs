namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

[EventHandlerQueue(TestConsts.Queues.Queue1.Name)]
internal class OutboxEventHandlerForQueue1 : OutboxEventHandlerBase, IOutboxEventHandler<OutboxEventForQueue1>
{
    public virtual Task HandleAsync(OutboxEventForQueue1 domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

[EventHandlerQueue(TestConsts.Queues.Queue2.Name)]
internal class OutboxEventHandlerForQueue2 : OutboxEventHandlerBase, IOutboxEventHandler<OutboxEventForQueue2>
{
    public virtual Task HandleAsync(OutboxEventForQueue2 domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

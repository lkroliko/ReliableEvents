namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

[EventHandlerQueue(TestConsts.Queues.Test.Name)]
internal class TestOutboxEventHandler : IOutboxEventHandler<TestOutboxEvent>
{
    public virtual Task HandleAsync(TestOutboxEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
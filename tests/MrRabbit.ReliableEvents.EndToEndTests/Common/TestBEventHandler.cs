namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestBEventHandler : IEventHandler<TestEvent>
{
    public virtual Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
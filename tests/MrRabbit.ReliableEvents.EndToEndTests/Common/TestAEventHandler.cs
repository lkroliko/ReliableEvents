namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestAEventHandler : IEventHandler<TestEvent>
{
    public virtual Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

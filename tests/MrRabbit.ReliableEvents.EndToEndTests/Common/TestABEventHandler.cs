namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestABEventHandler : IEventHandler<TestAEvent>, IEventHandler<TestBEvent>
{
    public virtual Task HandleAsync(TestAEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task HandleAsync(TestBEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
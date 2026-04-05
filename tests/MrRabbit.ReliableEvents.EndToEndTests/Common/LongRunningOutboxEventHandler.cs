namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

[EventHandlerQueue(TestConsts.Queues.Test.Name)]
internal class LongRunningOutboxEventHandler : IOutboxEventHandler<LongRunningOutboxEvent>
{
    public virtual async Task HandleAsync(LongRunningOutboxEvent @event, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    }
}

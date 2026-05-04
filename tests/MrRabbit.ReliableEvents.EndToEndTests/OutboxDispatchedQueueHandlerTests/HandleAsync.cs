namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatchedQueueHandlerTests;

[Trait("Category", "OutboxDispatchedQueueHandler")]
public class HandleAsync : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly IOutboxDispatchedQueueHandler _dispatchedHandler = Mock.Of<IOutboxDispatchedQueueHandler>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public HandleAsync(DatabaseFixture fixture) : base(fixture) { }

    override protected void ConfigureServiceProvider(IServiceCollection services)
    {
        services.AddSingleton(_dispatchedHandler);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHookAfterDispatchAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        Mock.Get(_dispatchedHandler).Verify(h => h.HandleAsync(It.Is<OutboxDispatchedQueueContext>(c => c.Queue.Name == TestConsts.Queues.Queue1.Name && c.DispatchedTasksCount == 1)), Times.Once);
    }
}

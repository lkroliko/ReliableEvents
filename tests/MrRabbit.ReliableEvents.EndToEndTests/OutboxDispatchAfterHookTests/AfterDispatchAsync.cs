namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatchAfterHookTests;

[Trait("Category", "OutboxDispatchAfterHook")]
public class AfterDispatchAsync : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly IOutboxDispatchAfterHook _hook = Mock.Of<IOutboxDispatchAfterHook>();
    private readonly CancellationToken _cancellationToken;

    public AfterDispatchAsync(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.AddSingleton(_hook);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHookAfterDispatchAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        Mock.Get(_hook).Verify(h => h.AfterDispatchAsync(It.Is<OutboxDispatchAfterContext>(c => c.Queue.Name == TestConsts.Queues.Queue1.Name && c.DispatchedTasksCount == 1)), Times.Once);
    }
}

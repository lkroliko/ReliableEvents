namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatchAfterHookTests;

[Trait("Category", "OutboxDispatchAfterHook")]
public class AfterDispatchAsync : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
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

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue], _cancellationToken);

        Mock.Get(_hook).Verify(h => h.AfterDispatchAsync(It.Is<OutboxDispatchAfterContext>(c => c.Queue.Name == TestConsts.Queues.Test.Name && c.DispatchedTasksCount == 1)), Times.Once);
    }
}

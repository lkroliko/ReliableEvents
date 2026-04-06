namespace MrRabbit.ReliableEvents.EndToEndTests.AfterOutboxDispatchHookTests;

[Trait("Category", "OutboxDispatchPostProcessor")]
public class AfterDispatchAsync : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
    private readonly IAfterOutboxDispatchHook _hook = Mock.Of<IAfterOutboxDispatchHook>();
    private readonly CancellationToken _cancellationToken;

    public AfterDispatchAsync(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.AddSingleton<IAfterOutboxDispatchHook>(_hook);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHookAfterDispatchAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.AddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue], _cancellationToken);

        Mock.Get(_hook).Verify(h => h.AfterDispatchAsync(It.Is<AfterOutboxDispatchContext>(c => c.Queue.Name == TestConsts.Queues.Test.Name && c.DispatchedTasksCount == 1)), Times.Once);
    }
}

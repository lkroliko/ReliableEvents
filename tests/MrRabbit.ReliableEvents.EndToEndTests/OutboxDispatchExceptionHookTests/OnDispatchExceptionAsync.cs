namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatchExceptionHookTests;

[Trait("Category", "OutboxDispatchExceptionHook")]
public class OnDispatchExceptionAsync : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
    private readonly TestOutboxEventHandler _handler = Mock.Of<TestOutboxEventHandler>();
    private readonly IOutboxDispatchExceptionHook _hook = Mock.Of<IOutboxDispatchExceptionHook>();
    private readonly CancellationToken _cancellationToken;
    private readonly Exception _exception = new Exception();

    public OnDispatchExceptionAsync(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<TestOutboxEventHandler>();
        services.AddScoped(_ => _handler);
        services.AddSingleton(_hook);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerThrowExceptionThenHookOnDispatchExceptionAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<TestOutboxEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(_exception);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue], _cancellationToken);

        Mock.Get(_hook).Verify(h => h.OnDispatchExceptionAsync(It.Is<OutboxDispatchExceptionContext>(c => c.Queue.Name == TestConsts.Queues.Test.Name && c.Exception == _exception)), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerNotThrowExceptionThenHookOnDispatchExceptionAsyncNotCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue], _cancellationToken);

        Mock.Get(_hook).Verify(h => h.OnDispatchExceptionAsync(It.IsAny<OutboxDispatchExceptionContext>()), Times.Never);
    }
}

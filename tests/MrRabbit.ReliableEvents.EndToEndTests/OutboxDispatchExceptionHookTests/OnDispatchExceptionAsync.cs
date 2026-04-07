namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatchExceptionHookTests;

[Trait("Category", "OutboxDispatchExceptionHook")]
public class OnDispatchExceptionAsync : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly OutboxEventHandlerForQueue1 _handler = Mock.Of<OutboxEventHandlerForQueue1>();
    private readonly IOutboxDispatchExceptionHook _hook = Mock.Of<IOutboxDispatchExceptionHook>();
    private readonly CancellationToken _cancellationToken;
    private readonly Exception _exception = new Exception();

    public OnDispatchExceptionAsync(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<OutboxEventHandlerForQueue1>();
        services.AddScoped(_ => _handler);
        services.AddSingleton(_hook);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerThrowExceptionThenHookOnDispatchExceptionAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(_exception);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        Mock.Get(_hook).Verify(h => h.OnDispatchExceptionAsync(It.Is<OutboxDispatchExceptionContext>(c => c.Queue.Name == TestConsts.Queues.Queue1.Name && c.Exception == _exception)), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerNotThrowExceptionThenHookOnDispatchExceptionAsyncNotCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        Mock.Get(_hook).Verify(h => h.OnDispatchExceptionAsync(It.IsAny<OutboxDispatchExceptionContext>()), Times.Never);
    }
}

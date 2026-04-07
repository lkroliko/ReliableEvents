namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatchErrorHandlerTests;

[Trait("Category", "OutboxDispatchErrorHandler")]
public class HandleAsync : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly OutboxEventHandlerForQueue1 _handler = Mock.Of<OutboxEventHandlerForQueue1>();
    private readonly IOutboxDispatchErrorHandler _errroHandler = Mock.Of<IOutboxDispatchErrorHandler>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly Exception _exception = new();

    public HandleAsync(DatabaseFixture fixture) : base(fixture) { }

    override protected void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<OutboxEventHandlerForQueue1>();
        services.AddScoped(_ => _handler);
        services.AddSingleton(_errroHandler);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerThrowExceptionThenHookOnDispatchExceptionAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(_exception);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        Mock.Get(_errroHandler).Verify(h => h.HandleAsync(It.Is<OutboxDispatchErrorContext>(c => c.Queue.Name == TestConsts.Queues.Queue1.Name && c.Exception == _exception)), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerNotThrowExceptionThenHookOnDispatchExceptionAsyncNotCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        Mock.Get(_errroHandler).Verify(h => h.HandleAsync(It.IsAny<OutboxDispatchErrorContext>()), Times.Never);
    }
}

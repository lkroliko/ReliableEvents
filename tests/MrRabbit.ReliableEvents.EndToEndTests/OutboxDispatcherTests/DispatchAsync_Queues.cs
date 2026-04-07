namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatcherTests;

[Trait("Category", "OutboxDispatcher")]
public class DispatchAsync_Queues : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly OutboxEventHandlerForQueue1 _handler = Mock.Of<OutboxEventHandlerForQueue1>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public DispatchAsync_Queues(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<OutboxEventHandlerForQueue1>();
        services.AddScoped(_ => _handler);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHandlersHandleAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue1>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenMultipleDispatchCalledThenHandlersHandleAsyncCalledOnce(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await Task.WhenAll(new[]
        {
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken)
        });

        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue1>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerThrowExceptionThenHandlersHandleAsyncCalledExactly(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await Task.WhenAll(new[]
        {
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken)
        });

        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue1>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchedThenResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        result.Should().HaveCount(1);
        result[0].IsSuccess.Should().BeTrue();
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenNotDispatchedThrowResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        result.Should().HaveCount(1);
        result[0].IsSuccess.Should().BeFalse();
    }
}

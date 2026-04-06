namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatcherTests;

[Trait("Category", "OutboxDispatcher")]
public class DispatchAsync : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
    private readonly TestOutboxEventHandler _handler = Mock.Of<TestOutboxEventHandler>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public DispatchAsync(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<TestOutboxEventHandler>();
        services.AddScoped(_ => _handler);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHandlersHandleAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.AddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue], _cancellationToken);

        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<TestOutboxEvent>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenMultipleDispatchCalledThenHandlersHandleAsyncCalledOnce(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.AddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await Task.WhenAll(new[]
        {
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken)
        });

        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<TestOutboxEvent>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerThrowExceptionThenHandlersHandleAsyncCalledExactly(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<TestOutboxEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
        await ReliableEvents.OutboxStore.AddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await Task.WhenAll(new[]
        {
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken)
        });

        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<TestOutboxEvent>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchedThenResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.AddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue], _cancellationToken);

        result.Should().HaveCount(1);
        result[0].IsSuccess.Should().BeTrue();
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenNotDispatchedThrowResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<TestOutboxEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
        await ReliableEvents.OutboxStore.AddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue], _cancellationToken);

        result.Should().HaveCount(1);
        result[0].IsSuccess.Should().BeFalse();
    }
}

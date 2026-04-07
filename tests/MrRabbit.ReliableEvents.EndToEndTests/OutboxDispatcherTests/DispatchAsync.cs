namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatcherTests;

[Trait("Category", "OutboxDispatcher")]
public class DispatchAsync : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event1 = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly OutboxEventForQueue2 _event2 = A.Fixture.Create<OutboxEventForQueue2>();
    private readonly OutboxEventHandlerForQueue1 _handler1 = Mock.Of<OutboxEventHandlerForQueue1>();
    private readonly OutboxEventHandlerForQueue2 _handler2 = Mock.Of<OutboxEventHandlerForQueue2>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly Exception _handler1Exception = new Exception();
    private readonly Exception _handler2Exception = new Exception();

    public DispatchAsync(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<OutboxEventHandlerForQueue1>();
        services.RemoveImplementedType<OutboxEventHandlerForQueue2>();
        services.AddScoped(_ => _handler1);
        services.AddScoped(_ => _handler2);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHandlersHandleAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

        Mock.Get(_handler1).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue1>(e => e.Data == _event1.Data), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Get(_handler2).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue2>(e => e.Data == _event2.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenMultipleDispatchCalledThenHandlersHandleAsyncCalledOnce(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

        await Task.WhenAll(new[]
        {
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
        });


        Mock.Get(_handler1).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue1>(e => e.Data == _event1.Data), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Get(_handler2).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue2>(e => e.Data == _event2.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerThrowExceptionThenHandlersHandleAsyncCalledExactly(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler1).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler1Exception);
        Mock.Get(_handler2).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue2>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler2Exception);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

        await Task.WhenAll(new[]
        {
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
        });

        Mock.Get(_handler1).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue1>(e => e.Data == _event1.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
        Mock.Get(_handler2).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue2>(e => e.Data == _event2.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchedThenResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue1.Name && r.IsSuccess);
        result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue2.Name && r.IsSuccess);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler2).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue2>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler2Exception);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue1.Name && r.IsSuccess);
        result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue2.Name && r.IsFailed && r.Exception == _handler2Exception);
    }


    [Theory]
    [DatabaseProviders]
    public async Task WhenNotDispatchedThenResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler1).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler1Exception);
        Mock.Get(_handler2).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue2>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler2Exception);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue1.Name && r.IsFailed && r.Exception == _handler1Exception);
        result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue2.Name && r.IsFailed && r.Exception == _handler2Exception);
    }
}

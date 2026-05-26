namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatcherTests;

[Trait("Category", "OutboxDispatcher")]
public class DispatchAsync_MultipleHandlers : ReliableEventsTestBase
{
    private readonly OutboxAEvent _eventA = A.Fixture.Create<OutboxAEvent>();
    private readonly OutboxBEvent _eventB = A.Fixture.Create<OutboxBEvent>();
    private readonly OutboxABEventHandlerForQueue1 _handlerAB = Mock.Of<OutboxABEventHandlerForQueue1>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public DispatchAsync_MultipleHandlers(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<OutboxABEventHandlerForQueue1>();
        services.AddScoped(_ => _handlerAB);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHandlersHandleAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);

        await ReliableEvents.OutboxStore.TryAddEventAsync(_eventA, _eventA.EventId, _eventA.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_eventB, _eventB.EventId, _eventB.OccurredDate, _cancellationToken);

        var resut = await ReliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

        Mock.Get(_handlerAB).Verify(h => h.HandleAsync(It.Is<OutboxAEvent>(e => e.Data == _eventA.Data), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Get(_handlerAB).Verify(h => h.HandleAsync(It.Is<OutboxBEvent>(e => e.Data == _eventB.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenMultipleDispatchCalledThenHandlersHandleAsyncCalledOnce(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_eventA, _eventA.EventId, _eventA.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_eventB, _eventB.EventId, _eventB.OccurredDate, _cancellationToken);

        await Task.WhenAll(new[]
        {
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
        });


        Mock.Get(_handlerAB).Verify(h => h.HandleAsync(It.Is<OutboxAEvent>(e => e.Data == _eventA.Data), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Get(_handlerAB).Verify(h => h.HandleAsync(It.Is<OutboxBEvent>(e => e.Data == _eventB.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    //[Theory]
    //[DatabaseProviders]
    //public async Task WhenHandlerThrowExceptionThenHandlersHandleAsyncCalledExactly(DatabaseProvider provider)
    //{
    //    Initialize(provider);
    //    Mock.Get(_handler1).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler1Exception);
    //    Mock.Get(_handler2).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue2>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler2Exception);
    //    await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
    //    await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

    //    await Task.WhenAll(new[]
    //    {
    //        Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
    //        Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
    //        Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync( _cancellationToken)),
    //    });

    //    Mock.Get(_handler1).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue1>(e => e.Data == _event1.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
    //    Mock.Get(_handler2).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue2>(e => e.Data == _event2.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
    //    Mock.Get(_handlerAB).Verify(h => h.HandleAsync(It.Is<OutboxAEvent>(e => e.Data == _eventA.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
    //    Mock.Get(_handlerAB).Verify(h => h.HandleAsync(It.Is<OutboxBEvent>(e => e.Data == _eventB.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
    //}

    //[Theory]
    //[DatabaseProviders]
    //public async Task WhenDispatchedThenResultIsValid(DatabaseProvider provider)
    //{
    //    Initialize(provider);
    //    await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
    //    await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

    //    var result = await ReliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

    //    result.Should().HaveCount(2);
    //    result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue1.Name && r.IsSuccess);
    //    result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue2.Name && r.IsSuccess);
    //}

    //[Theory]
    //[DatabaseProviders]
    //public async Task WhenDispatchThenResultIsValid(DatabaseProvider provider)
    //{
    //    Initialize(provider);
    //    Mock.Get(_handler2).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue2>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler2Exception);
    //    await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
    //    await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

    //    var result = await ReliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

    //    result.Should().HaveCount(2);
    //    result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue1.Name && r.IsSuccess);
    //    result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue2.Name && r.IsFailed && r.Exception == _handler2Exception);
    //}


    //[Theory]
    //[DatabaseProviders]
    //public async Task WhenNotDispatchedThenResultIsValid(DatabaseProvider provider)
    //{
    //    Initialize(provider);
    //    Mock.Get(_handler1).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler1Exception);
    //    Mock.Get(_handler2).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue2>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handler2Exception);
    //    await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
    //    await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

    //    var result = await ReliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

    //    result.Should().HaveCount(2);
    //    result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue1.Name && r.IsFailed && r.Exception == _handler1Exception);
    //    result.Should().ContainSingle(r => r.Queue.Name == TestConsts.Queues.Queue2.Name && r.IsFailed && r.Exception == _handler2Exception);
    //}

    //[Theory]
    //[DatabaseProviders]
    //public async Task WhenDispatchInDbContextScopeThenResultIsValid(DatabaseProvider provider)
    //{
    //    Initialize(provider);
    //    var scope = Services.CreateScope();
    //    var reliableEvents = scope.ServiceProvider.GetRequiredService<IReliableEvents<TestDbContext>>();
    //    var queues = reliableEvents.OutboxStore.AttachEvent(_event1, _event1.EventId, _event1.OccurredDate);
    //    var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
    //    dbContext.SaveChanges();

    //    var result = await reliableEvents.OutboxDispatcher.DispatchAsync(_cancellationToken);

    //    result.Should().HaveCount(1);
    //    result.Should().ContainSingle(r => r.IsSuccess && r.Queue == TestConsts.Queues.Queue1.Queue);
    //}
}

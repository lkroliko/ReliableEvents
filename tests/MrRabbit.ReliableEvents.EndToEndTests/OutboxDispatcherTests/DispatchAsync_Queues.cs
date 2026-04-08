namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatcherTests;

[Trait("Category", "OutboxDispatcher")]
public class DispatchAsync_Queues : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly OutboxEventHandlerForQueue1 _handler = Mock.Of<OutboxEventHandlerForQueue1>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly Exception _handlerException = new();

    public DispatchAsync_Queues(DatabaseFixture fixture) : base(fixture) { }

    override protected void ConfigureServiceProvider(IServiceCollection services)
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

        await Task.WhenAll(
            [
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken)
        ]);

        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<OutboxEventForQueue1>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenHandlerThrowExceptionThenHandlersHandleAsyncCalledExactly(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handlerException);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await Task.WhenAll(
        [
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue]), _cancellationToken)
        ]);

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
        result.Should().ContainSingle(r => r.IsSuccess && r.Queue == TestConsts.Queues.Queue1.Queue);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenNotDispatchedThrowResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<OutboxEventForQueue1>(), It.IsAny<CancellationToken>())).ThrowsAsync(_handlerException);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Queue1.Queue], _cancellationToken);

        result.Should().HaveCount(1);
        result.Should().ContainSingle(r => r.IsFailed && r.Exception == _handlerException && r.Queue == TestConsts.Queues.Queue1.Queue);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchInDbContextScopeThenResultIsValid(DatabaseProvider provider)
    {
        Initialize(provider);
        var scope = Services.CreateScope();
        var reliableEvents = scope.ServiceProvider.GetRequiredService<IReliableEvents<TestDbContext>>();
        var queues = reliableEvents.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.SaveChanges();

        var result = await reliableEvents.OutboxDispatcher.DispatchAsync(queues, _cancellationToken);

        result.Should().HaveCount(1);
        result.Should().ContainSingle(r => r.IsSuccess && r.Queue == TestConsts.Queues.Queue1.Queue);
    }
}

namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatcherTests;

[Trait("Category", "OutboxDispatcher")]
public class DispatchAsync : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
    private readonly TestOutboxEventHandler _handler = Mock.Of<TestOutboxEventHandler>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<TestOutboxEventHandler>();
        services.AddScoped(_ => _handler);
    }

    [Fact]
    public async Task WhenDispatchThenHandlersHandleAsyncCalled()
    {
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();
        eventingServicing.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();
        var outboxTask = dbContext.Set<OutboxTask>().First();

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]);

        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<TestOutboxEvent>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenMultipleDispatchCalledThenHandlersHandleAsyncCalledOnce()
    {
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();
        eventingServicing.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();

        _ = Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken);
        _ = Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken);
        _ = Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken);

        await Task.Delay(500, _cancellationToken);
        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<TestOutboxEvent>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenHandlerThrowExceptionThenHandlersHandleAsyncCalledExactly()
    {
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<TestOutboxEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();
        eventingServicing.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();

        var tasks = new[] {
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken),
            Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken)
        };

        await Task.WhenAll(tasks);
        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<TestOutboxEvent>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task WhenDispatchedThenResultIsValid()
    {
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();
        eventingServicing.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();
        var outboxTask = dbContext.Set<OutboxTask>().First();

        var result = await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]);

        result.Should().HaveCount(1);
        result[0].IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task WhenNotDispatchedThrowResultIsValid()
    {
        Mock.Get(_handler).Setup(h => h.HandleAsync(It.IsAny<TestOutboxEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();
        eventingServicing.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();

        var result = await Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]), _cancellationToken);

        result.Should().HaveCount(1);
        result[0].IsSuccess.Should().BeFalse();
    }
}

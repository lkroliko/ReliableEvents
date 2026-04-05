namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatcherTests;

[Trait("Category", "OutboxDispatcher")]
public class DispatchAsync : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
    private readonly TestOutboxEventHandler _handler = Mock.Of<TestOutboxEventHandler>();

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

        _ = Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]));
        _ = Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]));
        _ = Task.Run(() => ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue]));

        await Task.Delay(500);
        Mock.Get(_handler).Verify(h => h.HandleAsync(It.Is<TestOutboxEvent>(e => e.Data == _event.Data), It.IsAny<CancellationToken>()), Times.Once);
    }
}

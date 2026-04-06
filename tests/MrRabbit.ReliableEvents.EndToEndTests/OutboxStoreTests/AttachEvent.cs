using Microsoft.EntityFrameworkCore;

namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxStoreTests;

[Trait("Category", "OutboxStore")]
public class AttachEvent : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
    private readonly CancellationToken _cancellationToken;

    public AttachEvent(DatabaseFixture fixture) : base(fixture) { }

    [Theory]
    [DatabaseProviders]
    public void WhenDispatchThenHandlersHandleAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetReliableEvents();

        eventingServicing.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);

        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();
        var outboxTask = DbContext.Set<OutboxTask>().Single();
        outboxTask.EventId.Should().Be(_event.EventId);
        outboxTask.OccurredDate.Should().Be(_event.OccurredDate);
        outboxTask.QueueName.Should().Be(TestConsts.Queues.Test.Name);
        outboxTask.HandlerAssemblyName.Should().Be(TestConsts.Assembly.Name);
        outboxTask.HandlerFullName.Should().Contain(nameof(TestOutboxEventHandler));
        outboxTask.EventAssemblyName.Should().Be(TestConsts.Assembly.Name);
        outboxTask.EventFullName.Should().Contain(nameof(TestOutboxEvent));
        outboxTask.EventData.Should().Contain(_event.Data);
        outboxTask.IsDispatched.Should().BeFalse();
        outboxTask.Id.Should().NotBeEmpty();
    }

    [Theory]
    [DatabaseProviders]
    public void WhenAttachEventTwiceThenExceptionThrown(DatabaseProvider provider)
    {
        Initialize(provider);

        var reliableEvents = ReliableEvents;
        reliableEvents.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);

        var result = Record.Exception(() => reliableEvents.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate));

        result.Should().NotBeNull();
        result.Should().BeOfType<InvalidOperationException>();
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenAttachEventWithDuplicateEventIdThenExceptionThrown(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.AddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);
        var scope = Services.CreateScope();
        var reliableEvents = scope.ServiceProvider.GetReliableEvents();
        reliableEvents.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetDbContext();

        var result = Record.Exception(() => dbContext.SaveChanges());

        result.Should().NotBeNull();
        result.Should().BeOfType<DbUpdateException>();
    }
}

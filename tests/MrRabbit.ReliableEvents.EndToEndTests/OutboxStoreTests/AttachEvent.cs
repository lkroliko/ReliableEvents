namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxStoreTests;

[Trait("Category", "OutboxStore")]
public class AttachEvent : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
    private readonly Guid _eventId = A.Fixture.Create<Guid>();

    [Fact]
    public void WhenDispatchThenHandlersHandleAsyncCalled()
    {
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();

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

    [Fact]
    public void WhenAttachEventTwiceThenExceptionThrown()
    {
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();
        eventingServicing.OutboxStore.AttachEvent(_event, _eventId, _event.OccurredDate);

        var result = Record.Exception(() => eventingServicing.OutboxStore.AttachEvent(_event, _eventId, _event.OccurredDate));

        result.Should().NotBeNull();
        result.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void WhenAttachEventWithDuplicateEventIdThenExceptionThrown()
    {
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();
        eventingServicing.OutboxStore.AttachEvent(_event, _eventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();

        eventingServicing.OutboxStore.AttachEvent(_event, _eventId, _event.OccurredDate);
        dbContext.SaveChanges();

        dbContext.Set<OutboxTask>().Single().EventId.Should().Be(_eventId);
    }
}

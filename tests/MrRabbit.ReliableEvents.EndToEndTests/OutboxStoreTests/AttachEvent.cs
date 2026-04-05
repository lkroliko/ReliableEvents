namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxStoreTests;

[Trait("Category", "OutboxStore")]
public class AttachEvent : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();

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
}

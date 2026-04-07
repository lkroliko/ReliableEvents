namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxStoreTests;

[Trait("Category", "OutboxStore")]
public class AttachEvents : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1[] _events = A.Fixture.CreateMany<OutboxEventForQueue1>(2).ToArray();

    public AttachEvents(DatabaseFixture fixture) : base(fixture) { }

    [Theory]
    [DatabaseProviders]
    public void WhenDispatchThenHandlersHandleAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        using var scope = Services.CreateScope();
        var reliableEvents = scope.ServiceProvider.GetReliableEvents();

        reliableEvents.OutboxStore.AttachEvents<OutboxEventBase>(_events, e => e.EventId, e => e.OccurredDate);

        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();
        var outboxTasks = DbContext.Set<OutboxTask>().ToList();
        outboxTasks.Should().HaveCount(_events.Length);
        outboxTasks.Should().AllSatisfy(x => x.QueueName.Should().Be(TestConsts.Queues.Queue1.Name));
        outboxTasks.Should().AllSatisfy(x => x.QueueName.Should().Be(TestConsts.Queues.Queue1.Name));
        outboxTasks.Should().AllSatisfy(x => x.HandlerAssemblyName.Should().Be(TestConsts.Assembly.Name));
        outboxTasks.Should().AllSatisfy(x => x.HandlerFullName.Should().Contain(nameof(OutboxEventHandlerForQueue1)));
        outboxTasks.Should().AllSatisfy(x => x.EventAssemblyName.Should().Be(TestConsts.Assembly.Name));
        outboxTasks.Should().AllSatisfy(x => x.EventFullName.Should().Contain(nameof(OutboxEventForQueue1)));
        outboxTasks.Should().AllSatisfy(x => x.IsDispatched.Should().BeFalse());
        outboxTasks.Should().AllSatisfy(x => x.Id.Should().NotBeEmpty());
        outboxTasks.Should().Contain(x => x.EventId == _events[0].EventId && x.OccurredDate == _events[0].OccurredDate && x.EventData.Contains(_events[0].Data));
        outboxTasks.Should().Contain(x => x.EventId == _events[1].EventId && x.OccurredDate == _events[1].OccurredDate && x.EventData.Contains(_events[1].Data));
    }
}

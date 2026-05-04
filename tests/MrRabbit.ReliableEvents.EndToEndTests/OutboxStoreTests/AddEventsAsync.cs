namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxStoreTests;

[Trait("Category", "OutboxStore")]
public class AddEventsAsync : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1[] _events = [.. A.Fixture.CreateMany<OutboxEventForQueue1>(2)];
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public AddEventsAsync(DatabaseFixture fixture) : base(fixture) { }

    [Theory]
    [DatabaseProviders]
    public async Task WhenEventsNotExistThenOutboxTasksValid(DatabaseProvider provider)
    {
        Initialize(provider);

        await ReliableEvents.OutboxStore.AddEventsAsync<OutboxEventBase>(_events, e => e.EventId, e => e.OccurredDate, _cancellationToken);

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

    [Theory]
    [DatabaseProviders(0)]
    [DatabaseProviders(1)]
    public async Task WhenEventExistThenOutboxTasksValid(DatabaseProvider provider, int index)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_events[index], _events[index].EventId, _events[index].OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxStore.AddEventsAsync<OutboxEventBase>(_events, e => e.EventId, e => e.OccurredDate, _cancellationToken);

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

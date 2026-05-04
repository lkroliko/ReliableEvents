namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxStoreTests;

[Trait("Category", "OutboxStore")]
public class TryAddEventAsync : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly CancellationToken _cancellationToken;

    public TryAddEventAsync(DatabaseFixture fixture) : base(fixture) { }

    [Theory]
    [DatabaseProviders]
    public async Task WhenEventNotExistThenOutboxTaskIsValid(DatabaseProvider provider)
    {
        Initialize(provider);

        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        var outboxTask = DbContext.Set<OutboxTask>().Single();
        outboxTask.EventId.Should().Be(_event.EventId);
        outboxTask.OccurredDate.Should().Be(_event.OccurredDate);
        outboxTask.QueueName.Should().Be(TestConsts.Queues.Queue1.Name);
        outboxTask.HandlerAssemblyName.Should().Be(TestConsts.Assembly.Name);
        outboxTask.HandlerFullName.Should().Contain(nameof(OutboxEventHandlerForQueue1));
        outboxTask.EventAssemblyName.Should().Be(TestConsts.Assembly.Name);
        outboxTask.EventFullName.Should().Contain(nameof(OutboxEventForQueue1));
        outboxTask.EventData.Should().Contain(_event.Data);
        outboxTask.IsDispatched.Should().BeFalse();
        outboxTask.Id.Should().NotBeEmpty();
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenEventExistThenOutboxTaskNotDuplicated(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        await ReliableEvents.OutboxStore.TryAddEventAsync(_event, _event.EventId, _event.OccurredDate, _cancellationToken);

        var outboxTask = DbContext.Set<OutboxTask>().Single();
        outboxTask.EventId.Should().Be(_event.EventId);
        outboxTask.OccurredDate.Should().Be(_event.OccurredDate);
        outboxTask.QueueName.Should().Be(TestConsts.Queues.Queue1.Name);
        outboxTask.HandlerAssemblyName.Should().Be(TestConsts.Assembly.Name);
        outboxTask.HandlerFullName.Should().Contain(nameof(OutboxEventHandlerForQueue1));
        outboxTask.EventAssemblyName.Should().Be(TestConsts.Assembly.Name);
        outboxTask.EventFullName.Should().Contain(nameof(OutboxEventForQueue1));
        outboxTask.EventData.Should().Contain(_event.Data);
        outboxTask.IsDispatched.Should().BeFalse();
        outboxTask.Id.Should().NotBeEmpty();
    }
}

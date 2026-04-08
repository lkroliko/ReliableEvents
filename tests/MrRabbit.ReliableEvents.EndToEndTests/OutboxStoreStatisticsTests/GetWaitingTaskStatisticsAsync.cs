namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxStoreStatisticsTests;

[Trait("Category", "OutboxStoreStatistics")]
public class GetWaitingTaskStatisticsAsync : ReliableEventsTestBase
{
    private readonly OutboxEventForQueue1 _event1 = A.Fixture.Create<OutboxEventForQueue1>();
    private readonly OutboxEventForQueue2 _event2 = A.Fixture.Create<OutboxEventForQueue2>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetWaitingTaskStatisticsAsync(DatabaseFixture fixture) : base(fixture) { }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHandlersHandleAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event1, _event1.EventId, _event1.OccurredDate, _cancellationToken);
        await ReliableEvents.OutboxStore.TryAddEventAsync(_event2, _event2.EventId, _event2.OccurredDate, _cancellationToken);

        var result = await OutboxStoreStatistics.GetWaitingTaskStatisticsAsync();

        ((int)result).Should().Be(2);
        result.WaitingTasksCount.Should().Be(2);
        result.Queues.Should().HaveCount(2);
        result.Queues.Should().Contain(q => q.Queue == TestConsts.Queues.Queue1.Queue && q.WaitingTasksCount == 1);
        result.Queues.Should().Contain(q => q.Queue == TestConsts.Queues.Queue2.Queue && q.WaitingTasksCount == 1);
    }
}

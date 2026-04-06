namespace MrRabbit.ReliableEvents.EndToEndTests.OutboxDispatchPostProcessorTests;

[Trait("Category", "OutboxDispatchPostProcessor")]
public class ProcessAsync : ReliableEventsTestBase
{
    private readonly TestOutboxEvent _event = A.Fixture.Create<TestOutboxEvent>();
    private readonly TestOutboxDispatchPostProcessor _postProcessor = Mock.Of<TestOutboxDispatchPostProcessor>();
    private readonly CancellationToken _cancellationToken;

    public ProcessAsync(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.AddSingleton<IOutboxDispatchPostProcessor>(_postProcessor);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenPostProcessorProcessAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);
        using var scope = Services.CreateScope();
        var eventingServicing = scope.ServiceProvider.GetEventingService();
        eventingServicing.OutboxStore.AttachEvent(_event, _event.EventId, _event.OccurredDate);
        var dbContext = scope.ServiceProvider.GetDbContext();
        dbContext.SaveChanges();

        await ReliableEvents.OutboxDispatcher.DispatchAsync([TestConsts.Queues.Test.Queue], _cancellationToken);

        Mock.Get(_postProcessor).Verify(h => h.ProcessAsync(It.Is<PostOutboxDispatchContext>(c => c.Queue.Name == TestConsts.Queues.Test.Name && c.DispatchedTasksCount == 1)), Times.Once);
    }
}

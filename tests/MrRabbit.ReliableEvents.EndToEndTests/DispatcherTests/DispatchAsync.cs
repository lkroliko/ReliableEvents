namespace MrRabbit.ReliableEvents.EndToEndTests.DispatcherTests;

[Trait("Category", "Dispatcher")]
public class DispatchAsync : ReliableEventsTestBase
{
    private readonly TestAEventHandler _handlerA = Mock.Of<TestAEventHandler>();
    private readonly TestBEventHandler _handlerB = Mock.Of<TestBEventHandler>();
    private readonly CancellationToken _cancellationToken;
    private readonly TestEvent _event = A.Fixture.Create<TestEvent>();

    public DispatchAsync(DatabaseFixture fixture) : base(fixture) { }

    protected override void ConfigureServiceProvider(IServiceCollection services)
    {
        services.RemoveImplementedType<TestAEventHandler>();
        services.AddScoped<IEventHandler<TestEvent>>(_ => _handlerA);
        services.RemoveImplementedType<TestBEventHandler>();
        services.AddScoped<IEventHandler<TestEvent>>(_ => _handlerB);
    }

    [Theory]
    [DatabaseProviders]
    public async Task WhenDispatchThenHandlersHandleAsyncCalled(DatabaseProvider provider)
    {
        Initialize(provider);

        await ReliableEvents.Dispatcher.DispatchAsync(new[] { _event }, _cancellationToken);

        Mock.Get(_handlerA).Verify(x => x.HandleAsync(_event, _cancellationToken), Times.Once);
        Mock.Get(_handlerB).Verify(x => x.HandleAsync(_event, _cancellationToken), Times.Once);
    }
}

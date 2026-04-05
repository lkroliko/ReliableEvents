namespace MrRabbit.ReliableEvents.EndToEndTests.DispatcherTests;

[Trait("Category", "Dispatcher")]
public class DispatchAsync : ReliableEventsTestBase
{
    private TestAEventHandler _handlerA = Mock.Of<TestAEventHandler>();
    private TestBEventHandler _handlerB = Mock.Of<TestBEventHandler>();
    private CancellationToken _cancellationToken;

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
        var @event = A.Fixture.Create<TestEvent>();

        await ReliableEvents.Dispatcher.DispatchAsync(new[] { @event }, _cancellationToken);

        Mock.Get(_handlerA).Verify(x => x.HandleAsync(@event, _cancellationToken), Times.Once);
        Mock.Get(_handlerB).Verify(x => x.HandleAsync(@event, _cancellationToken), Times.Once);
    }
}

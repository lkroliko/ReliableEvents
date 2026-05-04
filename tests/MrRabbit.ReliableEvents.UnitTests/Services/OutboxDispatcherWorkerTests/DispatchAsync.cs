namespace MrRabbit.ReliableEvents.UnitTests.Services.OutboxDispatcherWorkerTests;

[Trait("Category", nameof(OutboxDispatcherWorker))]
public class DispatchAsync
{
    private readonly OutboxDispatcherWorker _worker;
    private readonly ISerializer _serializer = Mock.Of<ISerializer>();
    private readonly IServiceProvider _serviceProvider = Mock.Of<IServiceProvider>();
    private readonly OutboxQueue _queue = new("TestQueue");
    private readonly OutboxTask _outboxTask = new()
    {
        Id = A.Fixture.Create<Guid>(),
        OccurredDate = A.Fixture.Create<DateTime>(),
        EventAssemblyName = typeof(TestEvent).Assembly.GetName().Name!,
        EventFullName = typeof(TestEvent).FullName!,
        EventData = A.Fixture.Create<string>(),
        HandlerAssemblyName = typeof(TestEventHandler).Assembly.GetName().Name!,
        HandlerFullName = typeof(TestEventHandler).FullName!,
        QueueName = "TestQueue",
        EventId = null,
    };
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly TestEvent _testEvent = new();
    private readonly TestEventHandler _testEventHandler = Mock.Of<TestEventHandler>();
    private readonly Exception _handlerException = new("Handler exception");

    public DispatchAsync()
    {
        _worker = new(_serializer, _serviceProvider);

        Mock.Get(_serializer).Setup(s => s.Deserialize(_outboxTask.EventData, typeof(TestEvent))).Returns(_testEvent);
        Mock.Get(_serviceProvider).Setup(s => s.GetService(typeof(TestEventHandler))).Returns(_testEventHandler);
    }

    [Fact]
    public async Task WhenDispatchedThenResultIsValid()
    {
        var result = await _worker.DispatchAsync(_queue, _outboxTask, _cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Queue.Should().Be(_queue);
    }

    [Fact]
    public async Task WhenHandlerThrowExceptionThenResultIsValid()
    {
        Mock.Get(_testEventHandler).Setup(h => h.HandleAsync(_testEvent, _cancellationToken)).ThrowsAsync(_handlerException);

        var result = await _worker.DispatchAsync(_queue, _outboxTask, _cancellationToken);

        result.IsFailed.Should().BeTrue();
        result.Queue.Should().Be(_queue);
        result.Exception.Should().Be(_handlerException);
    }

    public class TestEvent { }

    public class TestEventHandler : IOutboxEventHandler<TestEvent>, IOutboxEventHandler<string>
    {
        public virtual Task HandleAsync(TestEvent @event, CancellationToken cancellationToken) =>
            Task.CompletedTask;
        public virtual Task HandleAsync(string @event, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}

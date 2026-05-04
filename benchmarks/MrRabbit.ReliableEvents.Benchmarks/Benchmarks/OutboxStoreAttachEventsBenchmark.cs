using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using MrRabbit.ReliableEvents.Attributes;
using MrRabbit.ReliableEvents.Benchmarks.Common;
using MrRabbit.ReliableEvents.Builders;

namespace MrRabbit.ReliableEvents.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class OutboxStoreAttachEventsBenchmark : BenchmarkBase
{
    private readonly DatabaseProvider _provider = DatabaseProvider.SQLite;
    private readonly Database _database;

    private IReliableEvents<BenchmarkDbContext>? _reliableEvents;
    private IServiceScope? _scope;
    private readonly List<OutboxEvent> _events = [];

    [Params(10_000)]
    public int EventCount;

    public OutboxStoreAttachEventsBenchmark()
    {
        _database = new(_provider);
    }

    [GlobalSetup]
    public void Setup() => SetupAsync().GetAwaiter().GetResult();

    private async Task SetupAsync()
    {
        _events.Clear();
        _events.AddRange(Enumerable.Range(0, EventCount).Select(_ => new OutboxEvent()));

        await _database.InitializeAsync();
        Initialize(_provider, _database.GetConnectionString());
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _scope = CreateScope();
        _reliableEvents = _scope.ServiceProvider.GetRequiredService<IReliableEvents<BenchmarkDbContext>>();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _scope?.Dispose();
        _scope = null;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        DisposeServices();
        _database.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    protected override void ConfigureReliableEvents(ReliableEventsOptionsBuilder options)
    {
        options.AddOutboxEventHandler<OutboxEventHandler>();
    }

    [Benchmark]
    public void AttachOutboxEvents()
    {
        _reliableEvents!.OutboxStore.AttachEvents(_events, e => e.EventId, e => e.OccurredDate);
    }

    [EventHandlerQueue(nameof(OutboxEventHandler))]
    public class OutboxEventHandler : IOutboxEventHandler<OutboxEvent>
    {
        public Task HandleAsync(OutboxEvent @event, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class OutboxEvent
    {
        public string EventId { get; } = Guid.NewGuid().ToString();
        public DateTime OccurredDate { get; } = DateTime.Now;
    }
}

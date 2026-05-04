using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using MrRabbit.ReliableEvents.Benchmarks.Common;

namespace MrRabbit.ReliableEvents.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class OutboxStoreAttachEventsWithoutHandlerBenchmark : BenchmarkBase
{
    private readonly DatabaseProvider _provider = DatabaseProvider.SQLite;
    private readonly Database _database;

    private IReliableEvents<BenchmarkDbContext>? _reliableEvents;
    private IServiceScope? _scope;
    private readonly List<OutboxEvent> _events = [];

    [Params(10_000)]
    public int EventCount;

    public OutboxStoreAttachEventsWithoutHandlerBenchmark()
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

    [Benchmark]
    public void AttachOutboxEventsWithoutHandler()
    {
        _reliableEvents!.OutboxStore.AttachEvents(_events, e => e.EventId, e => e.OccurredDate);
    }

    public class OutboxEvent
    {
        public string EventId { get; } = Guid.NewGuid().ToString();
        public DateTime OccurredDate { get; } = DateTime.Now;
    }
}

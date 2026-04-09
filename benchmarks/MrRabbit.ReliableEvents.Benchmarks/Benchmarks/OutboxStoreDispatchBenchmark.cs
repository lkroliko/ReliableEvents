using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MrRabbit.ReliableEvents.Attributes;
using MrRabbit.ReliableEvents.Benchmarks.Common;
using MrRabbit.ReliableEvents.Builders;

namespace MrRabbit.ReliableEvents.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 4, invocationCount: 2)]
public class OutboxStoreDispatchBenchmark : BenchmarkBase
{
    private readonly DatabaseProvider _provider = DatabaseProvider.MsSql;
    private readonly Database _database;

    private IReliableEvents<BenchmarkDbContext>? _reliableEvents;
    private IServiceScope? _scope;

    [Params(1_000, 10_000)]
    public int EventCount;

    public OutboxStoreDispatchBenchmark()
    {
        _database = new(_provider);
    }


    [GlobalSetup]
    public void Setup() => SetupAsync().GetAwaiter().GetResult();

    private async Task SetupAsync()
    {
        await _database.InitializeAsync();
        Initialize(_provider, _database.GetConnectionString());
    }

    protected override void ConfigureReliableEvents(ReliableEventsOptionsBuilder options)
    {
        options.AddOutboxEventHandler<OutboxEventHandler>();
    }

    private void AddEventsToStore()
    {
        var events = Enumerable.Range(0, EventCount).Select(_ => new OutboxEvent()).ToList();
        using var scope = CreateScope();
        var reliableEvents = scope.ServiceProvider.GetRequiredService<IReliableEvents<BenchmarkDbContext>>();
        reliableEvents.OutboxStore.AttachEvents(events, e => e.EventId, e => e.OccurredDate);
        var dbContext = scope.ServiceProvider.GetRequiredService<BenchmarkDbContext>();
        dbContext.Database.ExecuteSqlRaw("DELETE FROM [OutboxTask]");
        dbContext.SaveChanges();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        AddEventsToStore();

        _scope = CreateScope();
        _reliableEvents = _scope.ServiceProvider.GetRequiredService<IReliableEvents<BenchmarkDbContext>>();
    }

    [Benchmark]
    public void AddOutboxEvents()
    {
        _reliableEvents!.OutboxDispatcher.DispatchAsync(CancellationToken.None).GetAwaiter().GetResult();
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

    [EventHandlerQueue(nameof(AddOutboxEvents))]
    public class OutboxEventHandler : IOutboxEventHandler<OutboxEvent>
    {
        public Task HandleAsync(OutboxEvent @event, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class OutboxEvent
    {
        public string EventId { get; } = Guid.NewGuid().ToString();
        public DateTime OccurredDate { get; } = DateTime.UtcNow;
    }
}

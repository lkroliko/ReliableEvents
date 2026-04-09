using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MrRabbit.ReliableEvents.Builders;

namespace MrRabbit.ReliableEvents.Benchmarks.Common;

public abstract class BenchmarkBase
{
    protected IServiceProvider Services { get; private set; } = default!;

    public IReliableEvents<BenchmarkDbContext> ReliableEvents => GetScopedService<IReliableEvents<BenchmarkDbContext>>();
    public BenchmarkDbContext DbContext => GetScopedService<BenchmarkDbContext>();
    public IOutboxStoreStatistics<BenchmarkDbContext> OutboxStoreStatistics => GetScopedService<IOutboxStoreStatistics<BenchmarkDbContext>>();

    protected IServiceScope CreateScope() => Services.CreateScope();

    private TService GetScopedService<TService>() where TService : notnull
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected void DisposeServices()
    {
        if (Services is IDisposable disposable)
            disposable.Dispose();
    }

    protected virtual void ConfigureServiceProvider(IServiceCollection services) { }

    protected void Initialize(DatabaseProvider provider, string connectionString)
    {
        var services = new ServiceCollection();
        services.AddReliableEvents<BenchmarkDbContext>(options => {
            ConfigureReliableEvents(options);
        });

        AddDbContext(services, provider, connectionString);
        ConfigureServiceProvider(services);
        Services = services.BuildServiceProvider(true);
        RecreateDatabase();
    }

    protected virtual void ConfigureReliableEvents(ReliableEventsOptionsBuilder builder) { }

    private void RecreateDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BenchmarkDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    private void AddDbContext(IServiceCollection services, DatabaseProvider provider, string connectionString)
    {
        switch (provider)
        {
            case DatabaseProvider.SQLite:
                services.AddDbContext<BenchmarkDbContext>(options => options.UseSqlite(connectionString));
                break;
            case DatabaseProvider.MsSql:
                services.AddDbContext<BenchmarkDbContext>(options => options.UseSqlServer(connectionString));
                break;
            case DatabaseProvider.Postgres:
                services.AddDbContext<BenchmarkDbContext>(options => options.UseNpgsql(connectionString));
                break;
            default:
                throw new NotImplementedException();
        }
    }
}

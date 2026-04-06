using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

public abstract class ReliableEventsTestBase
{
    private readonly DatabaseFixture _fixture;

    protected IServiceProvider Services { get; private set; } = default!;

    public IReliableEvents<TestDbContext> ReliableEvents => GetScopedService<IReliableEvents<TestDbContext>>();
    public TestDbContext DbContext => GetScopedService<TestDbContext>();

    private TService GetScopedService<TService>() where TService : notnull
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected ReliableEventsTestBase(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    protected virtual void ConfigureServiceProvider(IServiceCollection services) { }

    protected void Initialize(DatabaseProvider provider)
    {
        var services = new ServiceCollection();
        services.AddReliableEvents<TestDbContext>(options => {
            options.AddEventHandlers(Assembly.GetExecutingAssembly());
            options.AddOutboxEventHandlers(Assembly.GetExecutingAssembly());
        });

        AddDbContext(services, provider);
        ConfigureServiceProvider(services);
        Services = services.BuildServiceProvider();
        RecreateDatabase();
    }

    private void RecreateDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    private void AddDbContext(IServiceCollection services, DatabaseProvider provider)
    {
        switch (provider)
        {
            case DatabaseProvider.SQLite:
                services.AddDbContext<TestDbContext>(options => options.UseSqlite(_fixture.GetConnectionString(provider)));
                break;
            case DatabaseProvider.MsSql:
                services.AddDbContext<TestDbContext>(options => options.UseSqlServer(_fixture.GetConnectionString(provider)));
                break;
            case DatabaseProvider.Postgres:
                services.AddDbContext<TestDbContext>(options => options.UseNpgsql(_fixture.GetConnectionString(provider)));
                break;
            default:
                throw new NotImplementedException();
        }
    }
}

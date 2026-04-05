using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

public abstract class ReliableEventsTestBase
{
    protected IServiceProvider Services { get; private set; } = default!;

    public IReliableEvents<TestDbContext> ReliableEvents => GetScopedService<IReliableEvents<TestDbContext>>();
    public TestDbContext DbContext => GetScopedService<TestDbContext>();

    private TService GetScopedService<TService>() where TService : notnull
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected ReliableEventsTestBase()
    {
        BuildServiceProvider();
    }

    protected virtual void ConfigureServiceProvider(IServiceCollection services) { }

    private void BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddReliableEvents<TestDbContext>(options => {
            options.AddEventHandlers(Assembly.GetExecutingAssembly());
            options.AddOutboxEventHandlers(Assembly.GetExecutingAssembly());
        });
        var databaseName = Guid.NewGuid().ToString();
        services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(databaseName));
        ConfigureServiceProvider(services);
        Services = services.BuildServiceProvider();
    }
}


namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class ScopedServiceEventHandler : IEventHandler<ScopedServiceEvent>
{
    private readonly IServiceProvider _services;
    private readonly TestDbContext _testDbContext;

    public ScopedServiceEventHandler(IServiceProvider services, TestDbContext testDbContext)
    {
        _services = services;
        _testDbContext = testDbContext;
    }

    public Task HandleAsync(ScopedServiceEvent @event, CancellationToken cancellationToken) => Task.CompletedTask;
}
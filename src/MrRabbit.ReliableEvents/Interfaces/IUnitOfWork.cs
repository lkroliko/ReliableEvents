namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IUnitOfWork<TDbContext> where TDbContext : DbContext
{
    IRepository Repository { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

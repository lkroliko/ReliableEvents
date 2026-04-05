namespace MrRabbit.ReliableEvents.Services;

internal sealed class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public IRepository Repository { get; }

    public UnitOfWork(TDbContext dbContext)
    {
        _dbContext = dbContext;

        Repository = new Repository(_dbContext);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await _dbContext.SaveChangesAsync(cancellationToken);
}

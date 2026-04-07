using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class OutboxDispatcher<TDbContext> : IOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<DispatchResult[]> DispatchAsync(IEnumerable<OutboxQueue> queues, CancellationToken cancellationToken) =>
        await Task.WhenAll(queues.Select(queue => DispatchAsync(queue, cancellationToken)));

    public async Task<DispatchResult> DispatchAsync(OutboxQueue queue, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var queueOutboxDispatcher = scope.ServiceProvider.GetRequiredService<IQueueOutboxDispatcher<TDbContext>>();
        return await queueOutboxDispatcher.DispatchAsync(queue, cancellationToken);
    }

    public async Task<DispatchResult[]> DispatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TDbContext>>();
        var queues = await _unitOfWork.Repository.GetQueuesToDispatchAsync();
        return await DispatchAsync(queues.Select(w => new OutboxQueue(w)), cancellationToken);
    }
}

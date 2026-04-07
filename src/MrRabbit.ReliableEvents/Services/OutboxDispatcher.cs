using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class OutboxDispatcher<TDbContext> : IOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    private readonly IQueueOutboxDispatcher<TDbContext> _outboxDispatcherWorker;
    private readonly IServiceProvider _serviceProvider;

    public OutboxDispatcher(IQueueOutboxDispatcher<TDbContext> outboxDispatcherWorker, IServiceProvider serviceProvider)
    {
        _outboxDispatcherWorker = outboxDispatcherWorker;
        _serviceProvider = serviceProvider;
    }

    public async Task<DispatchResult[]> DispatchAsync(IEnumerable<OutboxQueue> queues, CancellationToken cancellationToken) =>
        await Task.WhenAll(queues.Select(queue => _outboxDispatcherWorker.DispatchAsync(queue, cancellationToken)));

    public async Task<DispatchResult[]> DispatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TDbContext>>();
        var queues = await _unitOfWork.Repository.GetQueuesToDispatchAsync();
        return await DispatchAsync(queues.Select(w => new OutboxQueue(w)), cancellationToken);
    }
}

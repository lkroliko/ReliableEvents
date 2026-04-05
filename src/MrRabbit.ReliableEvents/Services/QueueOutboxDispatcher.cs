using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class QueueOutboxDispatcher<TDbContext> : IQueueOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public QueueOutboxDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(Queue queue, CancellationToken cancellationToken)
    {
        while (true)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TDbContext>>();
            var outboxTask = await unitOfWork.Repository.GetOldestOutboxTaskAsync(queue);
            if (outboxTask is null)
                break;

            var worker = scope.ServiceProvider.GetRequiredService<IOutboxDispatcherWorker>();
            await worker.DispatchAsync(outboxTask, cancellationToken);
            if (outboxTask.EventId is null)
                unitOfWork.Repository.Remove(outboxTask);
            else
                outboxTask.MarkAsDispatched();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                break;
        }
    }
}
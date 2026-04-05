using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class QueueOutboxDispatcher<TDbContext> : IQueueOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxQueueSemaphoreProvider<TDbContext> _semaphoreProvider;

    public QueueOutboxDispatcher(IServiceProvider serviceProvider, IOutboxQueueSemaphoreProvider<TDbContext> SemaphoreProvider)
    {
        _serviceProvider = serviceProvider;
        _semaphoreProvider = SemaphoreProvider;
    }

    public async Task<DispatchResult> DispatchAsync(OutboxQueue queue, CancellationToken cancellationToken)
    {
        var semapthore = _semaphoreProvider.Get(queue);
        await semapthore.WaitAsync(cancellationToken);
        try
        {
            while (true)
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TDbContext>>();
                var outboxTask = await unitOfWork.Repository.GetOldestOutboxTaskAsync(queue);
                if (outboxTask is null)
                    break;

                var worker = scope.ServiceProvider.GetRequiredService<IOutboxDispatcherWorker>();
                var result = await worker.DispatchAsync(queue, outboxTask, cancellationToken);
                if (result.IsFailed)
                    return result;

                if (outboxTask.EventId is null)
                    unitOfWork.Repository.Remove(outboxTask);
                else
                    outboxTask.MarkAsDispatched();
                await unitOfWork.SaveChangesAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            semapthore.Release();
        }

        return DispatchResult.Ok(queue);
    }
}

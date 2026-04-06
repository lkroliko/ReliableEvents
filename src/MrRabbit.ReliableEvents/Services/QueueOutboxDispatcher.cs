using Microsoft.Extensions.DependencyInjection;

namespace MrRabbit.ReliableEvents.Services;

internal class QueueOutboxDispatcher<TDbContext> : IQueueOutboxDispatcher<TDbContext> where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxQueueSemaphoreProvider<TDbContext> _semaphoreProvider;
    private readonly IOutboxDispatchHookInvoker _hookInvoker;

    public QueueOutboxDispatcher(IServiceProvider serviceProvider, IOutboxQueueSemaphoreProvider<TDbContext> SemaphoreProvider, IOutboxDispatchHookInvoker hookInvoker)
    {
        _serviceProvider = serviceProvider;
        _semaphoreProvider = SemaphoreProvider;
        _hookInvoker = hookInvoker;
    }

    public async Task<DispatchResult> DispatchAsync(OutboxQueue queue, CancellationToken cancellationToken)
    {
        var semapthore = _semaphoreProvider.Get(queue);
        await semapthore.WaitAsync(cancellationToken);
        var dispatchedTasksCount = 0;
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
                {
                    await _hookInvoker.InvokeExceptionHooksAsync(result);
                    return result;
                }

                if (outboxTask.EventId is null)
                    unitOfWork.Repository.Remove(outboxTask);
                else
                    outboxTask.MarkAsDispatched();
                await unitOfWork.SaveChangesAsync(cancellationToken);
                dispatchedTasksCount++;
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
            if (dispatchedTasksCount > 0)
                await _hookInvoker.InvokeAfterHooksAsync(queue, dispatchedTasksCount);
        }

        return DispatchResult.Ok(queue);
    }
}

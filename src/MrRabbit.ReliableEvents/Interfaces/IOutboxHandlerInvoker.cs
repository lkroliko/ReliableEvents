namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IOutboxHandlerInvoker
{
    Task InvokeDispatchedQueueHandlerAsync(OutboxQueue queue, int dispatchedTasksCount);
    Task InvokeDispatchQueueErrorHandlerAsync(DispatchResult result);
}

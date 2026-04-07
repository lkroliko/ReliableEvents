namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IOutboxHandlerInvoker
{
    Task InvokeDispatchedHandlerAsync(OutboxQueue queue, int dispatchedTasksCount);
    Task InvokeDispatchErrorHandlerAsync(DispatchResult result);
}

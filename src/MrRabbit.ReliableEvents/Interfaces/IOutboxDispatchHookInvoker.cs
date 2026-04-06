namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IOutboxDispatchHookInvoker
{
    Task InvokeAfterHooksAsync(OutboxQueue queue, int dispatchedTasksCount);
    Task InvokeExceptionHooksAsync(DispatchResult result);
}

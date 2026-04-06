namespace MrRabbit.ReliableEvents;

public class DispatchResult
{
    public OutboxQueue Queue { get; }
    public bool IsSuccess { get; }
    public bool IsFailed => IsSuccess == false;
    public Exception? Exception { get; }

    public DispatchResult(OutboxQueue queue, bool isSuccess, Exception? exception = null)
    {
        Queue = queue;
        IsSuccess = isSuccess;
        Exception = exception;
    }

    internal static DispatchResult Fail(OutboxQueue queue, Exception ex) => new DispatchResult(queue, false, ex);

    internal static DispatchResult Ok(OutboxQueue queue) => new DispatchResult(queue, true);
}

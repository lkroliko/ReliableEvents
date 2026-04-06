namespace MrRabbit.ReliableEvents;

public class ReliableEventsException : Exception
{
    public ReliableEventsException(string? message) : base(message) { }
    public ReliableEventsException(string? message, Exception? innerException) : base(message, innerException) { }
}

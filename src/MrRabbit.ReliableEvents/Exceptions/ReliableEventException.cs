namespace MrRabbit.ReliableEvents.Exceptions;

public class ReliableEventException : Exception
{
    public ReliableEventException(string? message) : base(message) { }
}

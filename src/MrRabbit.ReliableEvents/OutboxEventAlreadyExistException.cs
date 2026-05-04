namespace MrRabbit.ReliableEvents;

public class OutboxEventAlreadyExistException : ReliableEventsException
{
    public OutboxEventAlreadyExistException(Exception innerException) : base(null, innerException) { }
}
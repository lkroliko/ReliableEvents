namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

public abstract class DomainEventBase
{
    public DateTime OccurredDate { get; } = DateTime.Now;
}

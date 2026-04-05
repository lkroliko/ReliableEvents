namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal abstract class OutboxEventBase
{
    internal required Guid? EventId { get; init; }
    internal required DateTime OccurredDate { get; init; }
}

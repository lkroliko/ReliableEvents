namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal abstract class OutboxEventBase
{
    internal string EventId { get; } = A.Fixture.Create<string>();
    internal required DateTime OccurredDate { get; init; }
}

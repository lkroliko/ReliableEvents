namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class OutboxAEvent : OutboxEventBase
{
    public required string Data { get; init; }
}

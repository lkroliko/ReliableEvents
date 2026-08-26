namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class OutboxBEvent : OutboxEventBase
{
    public required string Data { get; init; }
}
namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestOutboxEvent : OutboxEventBase
{
    public required string Data { get; init; }
}

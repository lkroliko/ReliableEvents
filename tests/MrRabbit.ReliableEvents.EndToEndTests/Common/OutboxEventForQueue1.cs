namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class OutboxEventForQueue1 : OutboxEventBase
{
    public required string Data { get; init; }
}

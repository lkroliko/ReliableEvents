namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class OutboxEventForQueue2 : OutboxEventBase
{
    public required string Data { get; init; }
}

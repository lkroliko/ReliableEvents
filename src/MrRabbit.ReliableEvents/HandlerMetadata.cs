namespace MrRabbit.ReliableEvents;

internal class HandlerMetadata
{
    internal required OutboxQueue Queue { get; init; }
    internal required Type Type { get; init; }
}

namespace MrRabbit.ReliableEvents.Common;

internal class HandlerMetadata
{
    internal required OutboxQueue Queue { get; init; }
    internal required Type Type { get; init; }
}

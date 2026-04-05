namespace MrRabbit.ReliableEvents;

internal class HandlerMetadata
{
    internal required Queue Queue { get; init; }
    internal required Type Type { get; init; }
}

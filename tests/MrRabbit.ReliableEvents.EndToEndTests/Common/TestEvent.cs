namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestEvent : DomainEventBase
{
    internal required string Value { get; init; }
}


namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestBEvent : DomainEventBase
{
    internal required string Value { get; init; }
}
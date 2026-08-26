
namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

internal class TestAEvent : DomainEventBase
{
    internal required string Value { get; init; }
}

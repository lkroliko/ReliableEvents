namespace MrRabbit.ReliableEvents.EndToEndTests;

internal class TestConsts
{
    internal class Queues
    {
        internal class Test
        {
            internal const string Name = "TestQueue";
            internal static readonly OutboxQueue Queue = new OutboxQueue(Name);
        }
    }

    internal class Assembly
    {
        internal const string Name = "MrRabbit.ReliableEvents.EndToEndTests";
    }
}

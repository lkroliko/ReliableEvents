namespace MrRabbit.ReliableEvents.EndToEndTests;

internal class TestConsts
{
    internal class Queues
    {
        internal class Queue1
        {
            internal const string Name = "Queue1";
            internal static readonly OutboxQueue Queue = new OutboxQueue(Name);
        }

        internal class Queue2
        {
            internal const string Name = "Queue2";
            internal static readonly OutboxQueue Queue = new OutboxQueue(Name);
        }
    }

    internal class Assembly
    {
        internal const string Name = "MrRabbit.ReliableEvents.EndToEndTests";
    }
}

namespace MrRabbit.ReliableEvents.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EventHandlerQueueAttribute : Attribute
{
    public OutboxQueue Queue { get; protected set; } = default!;

    public EventHandlerQueueAttribute(string queueName)
    {
        Queue = new OutboxQueue(queueName);
    }

    public EventHandlerQueueAttribute(OutboxQueue queue)
    {
        Queue = queue;
    }
}

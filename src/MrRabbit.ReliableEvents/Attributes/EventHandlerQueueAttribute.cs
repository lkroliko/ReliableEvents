namespace MrRabbit.ReliableEvents.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EventHandlerQueueAttribute : Attribute
{
    public Queue Queue { get; protected set; } = default!;

    public EventHandlerQueueAttribute(string queueName)
    {
        Queue = new Queue(queueName);
    }

    public EventHandlerQueueAttribute(Queue queue)
    {
        Queue = queue;
    }
}

namespace MrRabbit.ReliableEvents;

public sealed class WaitingTaskStatistics
{
    public required int WaitingTasksCount { get; init; }
    public required IEnumerable<QueueWaitingTaskStatistics> Queues { get; init; }

    public class QueueWaitingTaskStatistics
    {
        public required OutboxQueue Queue { get; init; }
        public required int WaitingTasksCount { get; init; }
    }

    public static implicit operator int(WaitingTaskStatistics statistics) => statistics.WaitingTasksCount;
}
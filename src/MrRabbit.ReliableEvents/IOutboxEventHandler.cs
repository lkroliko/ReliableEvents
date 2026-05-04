namespace MrRabbit.ReliableEvents;

public interface IOutboxEventHandler<TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
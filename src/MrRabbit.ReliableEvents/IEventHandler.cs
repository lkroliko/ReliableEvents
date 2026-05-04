namespace MrRabbit.ReliableEvents;

public interface IEventHandler<TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}

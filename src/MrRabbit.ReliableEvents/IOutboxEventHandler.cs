namespace MrRabbit.ReliableEvents;

public interface IOutboxEventHandler<TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}

public interface IOutboxEventSuccessHandler<TEvent>//TODO czy to jest potrzebne??
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
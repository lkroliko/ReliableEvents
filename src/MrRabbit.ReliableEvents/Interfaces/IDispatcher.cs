namespace MrRabbit.ReliableEvents.Interfaces;

public interface IDispatcher
{
    Task DispatchAsync(IEnumerable<object> events, CancellationToken cancellationToken = default);
}

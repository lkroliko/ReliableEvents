namespace MrRabbit.ReliableEvents.Interfaces;

internal interface IHandlerMetadataProvider<TDbContext> where TDbContext : DbContext
{
    IEnumerable<HandlerMetadata> GetHandlersMetadata(object @event);
}

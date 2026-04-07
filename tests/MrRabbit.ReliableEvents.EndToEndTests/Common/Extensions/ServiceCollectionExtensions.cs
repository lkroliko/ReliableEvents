namespace MrRabbit.ReliableEvents.EndToEndTests.Common.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static void RemoveImplementedType<T>(this IServiceCollection services)
    {
        var descriptor = services.First(x => x.ImplementationType == typeof(T));
        services.Remove(descriptor);
    }

    internal static void RemoveImplementedType(this IServiceCollection services, Type type)
    {
        var descriptor = services.First(x => x.ImplementationType == type);
        services.Remove(descriptor);
    }
}

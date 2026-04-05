namespace MrRabbit.ReliableEvents.EndToEndTests.Common.Extensions;

internal static class ServiceProviderExtensions
{
    internal static IReliableEvents<TestDbContext> GetEventingService(this IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IReliableEvents<TestDbContext>>();

    internal static TestDbContext GetDbContext(this IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<TestDbContext>();
}

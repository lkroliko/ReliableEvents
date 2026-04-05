using System.Reflection;

namespace MrRabbit.ReliableEvents.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AddReliableEvents(this ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}

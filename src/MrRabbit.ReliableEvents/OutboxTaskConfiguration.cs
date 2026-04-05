using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MrRabbit.ReliableEvents;

internal class OutboxTaskConfiguration : IEntityTypeConfiguration<OutboxTask>
{
    public void Configure(EntityTypeBuilder<OutboxTask> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QueueName).IsRequired();
        builder.Property(x => x.HandlerAssemblyName).IsRequired();
        builder.Property(x => x.HandlerFullName).IsRequired();
        builder.Property(x => x.EventAssemblyName).IsRequired();
        builder.Property(x => x.EventFullName).IsRequired();
        builder.Property(x => x.EventData).IsRequired();
        builder.Property(x => x.OccurredDate).IsRequired();

        builder.HasIndex(x => new { x.QueueName, x.IsDispatched, x.OccurredDate });
        builder.HasIndex(x => x.EventId).IsUnique();
    }
}

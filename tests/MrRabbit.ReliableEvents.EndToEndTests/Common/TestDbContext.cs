using Microsoft.EntityFrameworkCore;
using MrRabbit.ReliableEvents.Extensions;

namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddReliableEvents();
    }
}

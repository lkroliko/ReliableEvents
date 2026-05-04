using Microsoft.EntityFrameworkCore;
using MrRabbit.ReliableEvents.Extensions;

namespace MrRabbit.ReliableEvents.Benchmarks.Common;

public class BenchmarkDbContext : DbContext
{
    public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddReliableEvents();
    }
}

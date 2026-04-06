using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;

namespace MrRabbit.ReliableEvents.EndToEndTests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly string _fileName = "test.db";
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .WithPortBinding(1433, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(1433))
        .WithCleanUp(true)
        .Build();

    public DatabaseFixture()
    {
        SQLitePCL.Batteries.Init();
    }

    public async ValueTask InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        File.Delete(_fileName);
        await _msSqlContainer.StopAsync();
    }

    public string GetConnectionString(DatabaseProvider provider) => provider switch
    {
        DatabaseProvider.SQLite => $"data source={_fileName}",
        DatabaseProvider.MsSql => _msSqlContainer.GetConnectionString().Replace("master", "ReliableEvents"),
        _ => throw new NotImplementedException()
    };
}
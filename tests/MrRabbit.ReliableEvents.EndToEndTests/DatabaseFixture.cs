using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace MrRabbit.ReliableEvents.EndToEndTests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly string _fileName = "test.db";
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .WithPortBinding(1433, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(1433))
        .WithCleanUp(true)
        .Build();

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("ReliableEvents")
        .WithPortBinding(5432, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
        .WithCleanUp(true)
        .Build();

    public DatabaseFixture()
    {
        SQLitePCL.Batteries.Init();
    }

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_msSqlContainer.StartAsync(), _postgresContainer.StartAsync());
    }

    public async ValueTask DisposeAsync()
    {
        File.Delete(_fileName);
        await Task.WhenAll(_msSqlContainer.StopAsync(), _postgresContainer.StopAsync());
    }

    public string GetConnectionString(DatabaseProvider provider) => provider switch
    {
        DatabaseProvider.SQLite => $"data source={_fileName}",
        DatabaseProvider.MsSql => _msSqlContainer.GetConnectionString().Replace("master", "ReliableEvents"),
        DatabaseProvider.Postgres => _postgresContainer.GetConnectionString(), //.Replace("postgres", "ReliableEvents"),
        _ => throw new NotImplementedException()
    };
}
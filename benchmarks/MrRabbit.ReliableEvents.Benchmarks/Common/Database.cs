using DotNet.Testcontainers.Builders;
using Microsoft.Data.Sqlite;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace MrRabbit.ReliableEvents.Benchmarks.Common;

public class Database
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

    private readonly DatabaseProvider _provider;

    public Database(DatabaseProvider database)
    {
        _provider = database;
    }

    public async ValueTask InitializeAsync()
    {
        switch (_provider)
        {
            case DatabaseProvider.SQLite:
                SQLitePCL.Batteries.Init();
                break;
            case DatabaseProvider.MsSql:
                await _msSqlContainer.StartAsync();
                break;
            case DatabaseProvider.Postgres:
                await _postgresContainer.StartAsync();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public async ValueTask DisposeAsync()
    {
        switch (_provider)
        {
            case DatabaseProvider.SQLite:
                SqliteConnection.ClearAllPools();
                File.Delete(_fileName);
                break;
            case DatabaseProvider.MsSql:
                await _msSqlContainer.StopAsync();
                break;
            case DatabaseProvider.Postgres:
                await _postgresContainer.StopAsync();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public string GetConnectionString() => _provider switch
    {
        DatabaseProvider.SQLite => $"data source={_fileName}",
        DatabaseProvider.MsSql => _msSqlContainer.GetConnectionString().Replace("master", "ReliableEvents"),
        DatabaseProvider.Postgres => _postgresContainer.GetConnectionString(),
        _ => throw new NotImplementedException()
    };
}
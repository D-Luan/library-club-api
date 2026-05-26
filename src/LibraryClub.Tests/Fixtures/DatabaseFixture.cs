using DbUp;
using Testcontainers.MsSql;

namespace LibraryClub.Tests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        ConnectionString = _dbContainer.GetConnectionString();

        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");

        EnsureDatabase.For.SqlDatabase(ConnectionString);

        var upgrader = DeployChanges.To
            .SqlDatabase(ConnectionString)
            .WithScriptsFromFileSystem(scriptsPath)
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new InvalidOperationException(
                "Database migration failed.",
                result.Error);
        }
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
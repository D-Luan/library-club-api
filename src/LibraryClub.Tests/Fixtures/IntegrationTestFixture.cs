using Dapper;
using LibraryClub.Api.Data;
using LibraryClub.Api.Repositories;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace LibraryClub.Tests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    private LibraryClubApiFactory? _factory;
    private HttpClient? _client;
    private ReaderRepository? _repository;

    public string ConnectionString { get; private set; } = string.Empty;

    public HttpClient Client => _client ?? throw new InvalidOperationException("Test client was not initialized.");

    public ReaderRepository Repository => _repository ?? throw new InvalidOperationException("Repository was not initialized.");

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        ConnectionString = _dbContainer.GetConnectionString();

        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");

        DatabaseMigrator.Migrate(ConnectionString, scriptsPath);

        _repository = new ReaderRepository(new SqlConnectionFactory(ConnectionString));

        _factory = new LibraryClubApiFactory(ConnectionString);
        _client = _factory.CreateClient();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);

        await connection.ExecuteAsync("""
              DELETE FROM ClubSubscriptions;
              DELETE FROM ReadingClubs;
              DELETE FROM Readers;
              """);
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();

        await _dbContainer.DisposeAsync();
    }
}
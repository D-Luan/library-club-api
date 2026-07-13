using Dapper;
using LibraryClub.Api.Data;
using LibraryClub.Api.Repositories;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace LibraryClub.Tests.Fixtures;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    private LibraryClubApiFactory? _factory;
    private HttpClient? _client;
    private ReaderRepository? _readerRepository;
    private ReadingClubRepository? _readingClubRepository;
    private ClubSubscriptionRepository? _clubSubscriptionRepository;

    public string ConnectionString { get; private set; } = string.Empty;

    public HttpClient Client =>
        _client ?? throw new InvalidOperationException(
            "Test client was not initialized.");

    public ReaderRepository ReaderRepository =>
        _readerRepository ?? throw new InvalidOperationException(
            "Reader repository was not initialized.");

    public ReadingClubRepository ReadingClubRepository =>
        _readingClubRepository ?? throw new InvalidOperationException(
            "Reading club repository was not initialized.");

    public ClubSubscriptionRepository ClubSubscriptionRepository =>
        _clubSubscriptionRepository ?? throw new InvalidOperationException(
            "Club subscription repository was not initialized.");

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        ConnectionString = _dbContainer.GetConnectionString();

        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");

        DatabaseMigrator.Migrate(ConnectionString, scriptsPath);

        var connectionFactory = new SqlConnectionFactory(ConnectionString);

        _readerRepository = new ReaderRepository(connectionFactory);
        _readingClubRepository = new ReadingClubRepository(connectionFactory);
        _clubSubscriptionRepository = new ClubSubscriptionRepository(connectionFactory);

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
using Dapper;
using LibraryClub.Api.Common;
using LibraryClub.Api.Data;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public sealed class ReadingClubRepository(ISqlConnectionFactory connectionFactory) 
    : IReadingClubRepository
{
    public async Task AddAsync(
        ReadingClub readingClub,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(readingClub);

        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO ReadingClubs (Id, Name, Description, Genre, Status, CreatedAt)
            VALUES (@Id, @Name, @Description, @Genre, @Status, @CreatedAt);
            """;

        var parameters = new
        {
            readingClub.Id,
            readingClub.Name,
            readingClub.Description,
            readingClub.Genre,
            Status = readingClub.Status.ToString(),
            readingClub.CreatedAt
        };

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }

    public async Task<ReadingClub?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, Name, Description, Genre, Status, CreatedAt
            FROM ReadingClubs
            WHERE Id = @Id;
            """;

        var record = await connection.QuerySingleOrDefaultAsync<ReadingClubRecord>(
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken));

        return record is null ? null : MapToReadingClub(record);
    }

    public async Task<PagedResult<ReadingClub>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        using var connection = connectionFactory.CreateConnection();

        var offset = (page - 1) * pageSize;

        const string sql = """
            SELECT COUNT(*)
            FROM ReadingClubs;

            SELECT Id, Name, Description, Genre, Status, CreatedAt
            FROM ReadingClubs
            ORDER BY CreatedAt DESC, Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            Offset = offset,
            PageSize = pageSize
        };

        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));

        var totalCount = await result.ReadSingleAsync<int>();

        var readingClubs = (await result.ReadAsync<ReadingClubRecord>())
            .Select(MapToReadingClub)
            .ToList();

        return new PagedResult<ReadingClub>(
            readingClubs,
            page,
            pageSize,
            totalCount);
    }

    public async Task UpdateAsync(
        ReadingClub readingClub,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(readingClub);

        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            UPDATE ReadingClubs
            SET Name = @Name,
                Description = @Description,
                Genre = @Genre,
                Status = @Status
            WHERE Id = @Id;
            """;

        var parameters = new
        {
            readingClub.Id,
            readingClub.Name,
            readingClub.Description,
            readingClub.Genre,
            Status = readingClub.Status.ToString()
        };

        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Reading club not found");
        }
    }

    private static ReadingClub MapToReadingClub(ReadingClubRecord record)
    {
        if (!Enum.TryParse<ReadingClubStatus>(record.Status, out var status) ||
            !Enum.IsDefined(status))
        {
            throw new InvalidOperationException($"Invalid reading club status: {record.Status}");
        }

        return ReadingClub.Restore(
            record.Id,
            record.Name,
            record.Description,
            record.Genre,
            status,
            record.CreatedAt);
    }

    private sealed record ReadingClubRecord(
        Guid Id,
        string Name,
        string? Description,
        string Genre,
        string Status,
        DateTime CreatedAt);
}

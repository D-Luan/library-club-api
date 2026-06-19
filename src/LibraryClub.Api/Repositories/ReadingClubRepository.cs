using Dapper;
using LibraryClub.Api.Common;
using LibraryClub.Api.Data;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public class ReadingClubRepository(ISqlConnectionFactory connectionFactory) : IReadingClubRepository
{
    public async Task AddAsync(ReadingClub readingClub)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
              INSERT INTO ReadingClubs (Id, Name, Description, Genre, Status, CreatedAt)
              VALUES (@Id, @Name, @Description, @Genre, @Status, @CreatedAt);
              """;

        await connection.ExecuteAsync(sql, new
        {
            readingClub.Id,
            readingClub.Name,
            readingClub.Description,
            readingClub.Genre,
            Status = readingClub.Status.ToString(),
            readingClub.CreatedAt
        });
    }

    public async Task<ReadingClub?> GetByIdAsync(Guid id)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
              SELECT Id, Name, Description, Genre, Status, CreatedAt
              FROM ReadingClubs
              WHERE Id = @Id
              """;

        var record = await connection.QuerySingleOrDefaultAsync<ReadingClubRecord>(sql, new { Id = id });

        return record is null ? null : MapToReadingClub(record);
    }

    public async Task UpdateAsync(ReadingClub readingClub)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
              UPDATE ReadingClubs
              SET Name = @Name,
                  Description = @Description,
                  Genre = @Genre,
                  Status = @Status
              WHERE Id = @Id
              """;

        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            readingClub.Id,
            readingClub.Name,
            readingClub.Description,
            readingClub.Genre,
            Status = readingClub.Status.ToString()
        });

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Reading club not found");
        }
    }

    public async Task<PagedResult<ReadingClub>> GetPagedAsync(int page, int pageSize)
    {
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

        using var result = await connection.QueryMultipleAsync(sql, new
        {
            Offset = offset,
            PageSize = pageSize
        });

        var totalCount = await result.ReadSingleAsync<int>();
        var records = (await result.ReadAsync<ReadingClubRecord>()).ToList();

        return new PagedResult<ReadingClub>(
            records.Select(MapToReadingClub).ToList(),
            page,
            pageSize,
            totalCount);
    }

    private static ReadingClub MapToReadingClub(ReadingClubRecord record)
    {
        if (!Enum.TryParse<ReadingClubStatus>(record.Status, out var status))
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

using Dapper;
using LibraryClub.Api.Common;
using LibraryClub.Api.Data;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public sealed class ReaderRepository(ISqlConnectionFactory connectionFactory) : IReaderRepository
{
    public async Task AddAsync(Reader reader, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);

        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO Readers (Id, Name, Email, Status, CreatedAt)
            VALUES (@Id, @Name, @Email, @Status, @CreatedAt);
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    reader.Id,
                    reader.Name,
                    reader.Email,
                    Status = reader.Status.ToString(),
                    reader.CreatedAt
                },
                cancellationToken: cancellationToken));
    }

    public async Task<Reader?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, Name, Email, Status, CreatedAt
            FROM Readers
            WHERE Id = @Id;
            """;

        var record = await connection.QuerySingleOrDefaultAsync<ReaderRecord>(
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken));

        return record is null ? null : MapToReader(record);
    }

    public async Task<Reader?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, Name, Email, Status, CreatedAt
            FROM Readers
            WHERE Email = @Email;
            """;

        var record = await connection.QuerySingleOrDefaultAsync<ReaderRecord>(
            new CommandDefinition(
                sql,
                new { Email = NormalizeEmail(email) },
                cancellationToken: cancellationToken));

        return record is null ? null : MapToReader(record);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT CAST(CASE WHEN EXISTS (
                SELECT 1
                FROM Readers
                WHERE Email = @Email
            ) THEN 1 ELSE 0 END AS bit);
            """;

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { Email = NormalizeEmail(email) },
                cancellationToken: cancellationToken));
    }

    public async Task<PagedResult<Reader>> GetPagedAsync(
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
            FROM Readers;

            SELECT Id, Name, Email, Status, CreatedAt
            FROM Readers
            ORDER BY CreatedAt DESC, Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new
                {
                    Offset = offset,
                    PageSize = pageSize
                },
                cancellationToken: cancellationToken));

        var totalCount = await result.ReadSingleAsync<int>();

        var readers = (await result.ReadAsync<ReaderRecord>())
            .Select(MapToReader)
            .ToList();

        return new PagedResult<Reader>(
            readers,
            page,
            pageSize,
            totalCount);
    }

    public async Task UpdateAsync(Reader reader, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);

        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            UPDATE Readers
            SET Name = @Name,
                Email = @Email,
                Status = @Status
            WHERE Id = @Id;
            """;

        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    reader.Id,
                    reader.Name,
                    reader.Email,
                    Status = reader.Status.ToString()
                },
                cancellationToken: cancellationToken));

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Reader not found");
        }
    }

    private static Reader MapToReader(ReaderRecord record)
    {
        if (!Enum.TryParse<ReaderStatus>(record.Status, out var status))
        {
            throw new InvalidOperationException($"Invalid reader status: {record.Status}");
        }

        return Reader.Restore(
            record.Id,
            record.Name,
            record.Email,
            status,
            record.CreatedAt);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private sealed record ReaderRecord(
        Guid Id,
        string Name,
        string Email,
        string Status,
        DateTime CreatedAt);
}

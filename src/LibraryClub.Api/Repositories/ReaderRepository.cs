using Dapper;
using LibraryClub.Api.Data;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public class ReaderRepository(ISqlConnectionFactory connectionFactory) : IReaderRepository
{
    public async Task AddAsync(Reader reader)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """    
            INSERT INTO Readers (Id, Name, Email, Status, CreatedAt)
            VALUES (@Id, @Name, @Email, @Status, @CreatedAt);            
            """;

        await connection.ExecuteAsync(sql, new
        {
            reader.Id,
            reader.Name,
            reader.Email,
            Status = reader.Status.ToString(),
            reader.CreatedAt
        });
    }

    public async Task<Reader?> GetByIdAsync(Guid id)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
              SELECT Id, Name, Email, Status, CreatedAt
              FROM Readers
              WHERE Id = @Id
              """;

        var record = await connection.QuerySingleOrDefaultAsync<ReaderRecord>(
            sql, 
            new { Id = id });

        return record is null ? null : MapToReader(record);
    }

    public async Task<Reader?> GetByEmailAsync(string email)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
              SELECT Id, Name, Email, Status, CreatedAt
              FROM Readers
              WHERE Email = @Email
              """;

        var record = await connection.QuerySingleOrDefaultAsync<ReaderRecord>(
            sql,
            new { Email = NormalizeEmail(email) });

        return record is null ? null : MapToReader(record);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
              SELECT CAST(CASE WHEN EXISTS (
                  SELECT 1
                  FROM Readers
                  WHERE Email = @Email
              ) THEN 1 ELSE 0 END AS bit)
              """;

        return await connection.ExecuteScalarAsync<bool>(
            sql,
            new { Email = NormalizeEmail(email) });
    }

    public async Task UpdateAsync(Reader reader)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
              UPDATE Readers
              SET Name = @Name,
                  Email = @Email,
                  Status = @Status
              WHERE Id = @Id
              """;

        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            reader.Id,
            reader.Name,
            reader.Email,
            Status = reader.Status.ToString()
        });

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

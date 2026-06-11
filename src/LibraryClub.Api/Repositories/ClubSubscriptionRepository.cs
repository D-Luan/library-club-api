using Dapper;
using LibraryClub.Api.Data;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public class ClubSubscriptionRepository(ISqlConnectionFactory connectionFactory) : IClubSubscriptionRepository
{
    public async Task AddAsync(ClubSubscription subscription)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                INSERT INTO ClubSubscriptions (Id, ReaderId, ReadingClubId, Status, CreatedAt,
                CanceledAt)
                VALUES (@Id, @ReaderId, @ReadingClubId, @Status, @CreatedAt, @CanceledAt);
                """;

        await connection.ExecuteAsync(sql, new
        {
            subscription.Id,
            subscription.ReaderId,
            subscription.ReadingClubId,
            Status = subscription.Status.ToString(),
            subscription.CreatedAt,
            subscription.CanceledAt
        });
    }

    public async Task<ClubSubscription?> GetByIdAsync(Guid id)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                SELECT Id, ReaderId, ReadingClubId, Status, CreatedAt, CanceledAt
                FROM ClubSubscriptions
                WHERE Id = @Id
                """;

        var record = await connection.QuerySingleOrDefaultAsync<ClubSubscriptionRecord>(sql, new { Id = id });

        return record is null ? null : MapToClubSubscription(record);
    }

    public async Task<bool> ExistsActiveAsync(Guid readerId, Guid readingClubId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                SELECT CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM ClubSubscriptions
                    WHERE ReaderId = @ReaderId
                      AND ReadingClubId = @ReadingClubId
                      AND Status = 'Active'
                ) THEN 1 ELSE 0 END AS bit)
                """;

        return await connection.ExecuteScalarAsync<bool>(sql, new
        {
            ReaderId = readerId,
            ReadingClubId = readingClubId
        });
    }

    public async Task UpdateAsync(ClubSubscription subscription)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                UPDATE ClubSubscriptions
                SET Status = @Status,
                    CanceledAt = @CanceledAt
                WHERE Id = @Id
                """;

        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            subscription.Id,
            Status = subscription.Status.ToString(),
            subscription.CanceledAt
        });

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Club subscription not found");
        }
    }

    private static ClubSubscription MapToClubSubscription(ClubSubscriptionRecord record)
    {
        if (!Enum.TryParse<ClubSubscriptionStatus>(record.Status, out var status))
        {
            throw new InvalidOperationException($"Invalid club subscription status: {record.Status}");
        }

        return ClubSubscription.Restore(
            record.Id,
            record.ReaderId,
            record.ReadingClubId,
            status,
            record.CreatedAt,
            record.CanceledAt);
    }

    private sealed record ClubSubscriptionRecord(
        Guid Id,
        Guid ReaderId,
        Guid ReadingClubId,
        string Status,
        DateTime CreatedAt,
        DateTime? CanceledAt);
}

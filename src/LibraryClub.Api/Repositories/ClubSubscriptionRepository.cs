using Dapper;
using LibraryClub.Api.Common;
using LibraryClub.Api.Data;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public sealed class ClubSubscriptionRepository(ISqlConnectionFactory connectionFactory)
    : IClubSubscriptionRepository
{
    public async Task AddAsync(
        ClubSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO ClubSubscriptions (
                Id,
                ReaderId,
                ReadingClubId,
                Status,
                CreatedAt,
                CanceledAt
            )
            VALUES (
                @Id,
                @ReaderId,
                @ReadingClubId,
                @Status,
                @CreatedAt,
                @CanceledAt
            );
            """;

        var parameters = new
        {
            subscription.Id,
            subscription.ReaderId,
            subscription.ReadingClubId,
            Status = subscription.Status.ToString(),
            subscription.CreatedAt,
            subscription.CanceledAt
        };

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }

    public async Task<ClubSubscription?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, ReaderId, ReadingClubId, Status, CreatedAt, CanceledAt
            FROM ClubSubscriptions
            WHERE Id = @Id;
            """;

        var record = await connection.QuerySingleOrDefaultAsync<ClubSubscriptionRecord>(
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken));

        return record is null ? null : MapToClubSubscription(record);
    }

    public async Task<PagedResult<ClubSubscription>> GetByReaderAsync(
        Guid readerId,
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
            FROM ClubSubscriptions
            WHERE ReaderId = @ReaderId;

            SELECT Id, ReaderId, ReadingClubId, Status, CreatedAt, CanceledAt
            FROM ClubSubscriptions
            WHERE ReaderId = @ReaderId
            ORDER BY CreatedAt DESC, Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            ReaderId = readerId,
            Offset = offset,
            PageSize = pageSize
        };

        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));

        var totalCount = await result.ReadSingleAsync<int>();

        var subscriptions = (await result.ReadAsync<ClubSubscriptionRecord>())
            .Select(MapToClubSubscription)
            .ToList();

        return new PagedResult<ClubSubscription>(
            subscriptions,
            page,
            pageSize,
            totalCount);
    }

    public async Task<PagedResult<ClubSubscription>> GetByReadingClubAsync(
        Guid readingClubId,
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
            FROM ClubSubscriptions
            WHERE ReadingClubId = @ReadingClubId;

            SELECT Id, ReaderId, ReadingClubId, Status, CreatedAt, CanceledAt
            FROM ClubSubscriptions
            WHERE ReadingClubId = @ReadingClubId
            ORDER BY CreatedAt DESC, Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            ReadingClubId = readingClubId,
            Offset = offset,
            PageSize = pageSize
        };

        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));

        var totalCount = await result.ReadSingleAsync<int>();

        var subscriptions = (await result.ReadAsync<ClubSubscriptionRecord>())
            .Select(MapToClubSubscription)
            .ToList();

        return new PagedResult<ClubSubscription>(
            subscriptions,
            page,
            pageSize,
            totalCount);
    }

    public async Task<bool> ExistsActiveAsync(
        Guid readerId,
        Guid readingClubId,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT CAST(CASE WHEN EXISTS (
                SELECT 1
                FROM ClubSubscriptions
                WHERE ReaderId = @ReaderId
                  AND ReadingClubId = @ReadingClubId
                  AND Status = @Status
            ) THEN 1 ELSE 0 END AS bit);
            """;

        var parameters = new
        {
            ReaderId = readerId,
            ReadingClubId = readingClubId,
            Status = ClubSubscriptionStatus.Active.ToString()
        };

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(
        ClubSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            UPDATE ClubSubscriptions
            SET Status = @Status,
                CanceledAt = @CanceledAt
            WHERE Id = @Id;
            """;

        var parameters = new
        {
            subscription.Id,
            Status = subscription.Status.ToString(),
            subscription.CanceledAt
        };

        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Club subscription not found");
        }
    }

    private static ClubSubscription MapToClubSubscription(ClubSubscriptionRecord record)
    {
        if (!Enum.TryParse<ClubSubscriptionStatus>(record.Status, out var status) ||
            !Enum.IsDefined(status))
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

using LibraryClub.Api.Common;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;

namespace LibraryClub.Api.Services;

public sealed class ClubSubscriptionService(
    IClubSubscriptionRepository subscriptionRepository,
    IReaderRepository readerRepository,
    IReadingClubRepository readingClubRepository,
    ILogger<ClubSubscriptionService> logger) : IClubSubscriptionService
{
    public async Task<ClubSubscription> CreateAsync(
        Guid readerId,
        Guid readingClubId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Creating club subscription for reader {ReaderId} and reading club {ReadingClubId}",
            readerId,
            readingClubId);

        var reader = await GetRequiredReaderAsync(readerId, cancellationToken);
        var readingClub = await GetRequiredReadingClubAsync(readingClubId, cancellationToken);

        EnsureReaderCanSubscribe(reader);
        EnsureReadingClubCanReceiveSubscriptions(readingClub);

        var activeSubscriptionExists = await subscriptionRepository.ExistsActiveAsync(
            readerId,
            readingClubId,
            cancellationToken);

        if (activeSubscriptionExists)
        {
            throw new ConflictException("Reader already has an active subscription for this reading club");
        }

        var subscription = new ClubSubscription(readerId, readingClubId);

        await subscriptionRepository.AddAsync(subscription, cancellationToken);

        logger.LogInformation(
            "Club subscription {ClubSubscriptionId} created successfully for reader {ReaderId} and reading club {ReadingClubId}",
            subscription.Id,
            readerId,
            readingClubId);

        return subscription;
    }

    public Task<ClubSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return subscriptionRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<PagedResult<ClubSubscription>> GetByReaderAsync(
        Guid readerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Listing subscriptions for reader {ReaderId} page {Page} pageSize {PageSize}",
            readerId,
            page,
            pageSize);

        await GetRequiredReaderAsync(readerId, cancellationToken);

        return await subscriptionRepository.GetByReaderAsync(
            readerId,
            page,
            pageSize,
            cancellationToken);
    }

    public async Task<PagedResult<ClubSubscription>> GetByReadingClubAsync(
        Guid readingClubId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Listing subscriptions for reading club {ReadingClubId} page {Page} pageSize {PageSize}",
            readingClubId,
            page,
            pageSize);

        await GetRequiredReadingClubAsync(readingClubId, cancellationToken);

        return await subscriptionRepository.GetByReadingClubAsync(
            readingClubId,
            page,
            pageSize,
            cancellationToken);
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Canceling club subscription {ClubSubscriptionId}", id);

        var subscription = await GetRequiredSubscriptionAsync(id, cancellationToken);

        subscription.Cancel();

        await subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        logger.LogInformation("Club subscription {ClubSubscriptionId} canceled successfully", id);
    }

    private async Task<Reader> GetRequiredReaderAsync(Guid readerId, CancellationToken cancellationToken)
    {
        var reader = await readerRepository.GetByIdAsync(readerId, cancellationToken);

        if (reader is null)
        {
            throw new NotFoundException("Reader not found");
        }

        return reader;
    }

    private async Task<ReadingClub> GetRequiredReadingClubAsync(Guid readingClubId, CancellationToken cancellationToken)
    {
        var readingClub = await readingClubRepository.GetByIdAsync(readingClubId, cancellationToken);

        if (readingClub is null)
        {
            throw new NotFoundException("Reading club not found");
        }

        return readingClub;
    }

    private async Task<ClubSubscription> GetRequiredSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);

        if (subscription is null)
        {
            throw new NotFoundException("Club subscription not found");
        }

        return subscription;
    }

    private static void EnsureReaderCanSubscribe(Reader reader)
    {
        if (reader.Status == ReaderStatus.Inactive)
        {
            throw new ConflictException("Reader is inactive");
        }
    }

    private static void EnsureReadingClubCanReceiveSubscriptions(ReadingClub readingClub)
    {
        if (readingClub.Status == ReadingClubStatus.Inactive)
        {
            throw new ConflictException("Reading club is inactive");
        }

        if (readingClub.Status == ReadingClubStatus.Archived)
        {
            throw new ConflictException("Reading club is archived");
        }
    }
}
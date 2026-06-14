using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;

namespace LibraryClub.Api.Services;

public class ClubSubscriptionService(
    IClubSubscriptionRepository subscriptionRepository,
    IReaderRepository readerRepository,
    IReadingClubRepository readingClubRepository,
    ILogger<ClubSubscriptionService> logger) : IClubSubscriptionService
{
    public async Task<ClubSubscription> CreateAsync(Guid readerId, Guid readingClubId)
    {
        logger.LogInformation("Creating club subscription for reader {ReaderId} and reading club {ReadingClubId}",
              readerId,
              readingClubId);

        var reader = await readerRepository.GetByIdAsync(readerId);

        if (reader is null)
        {
            throw new NotFoundException("Reader not found");
        }

        var readingClub = await readingClubRepository.GetByIdAsync(readingClubId);

        if (readingClub is null)
        {
            throw new NotFoundException("Reading club not found");
        }

        if (reader.Status == ReaderStatus.Inactive)
        {
            throw new ConflictException("Reader is inactive");
        }

        if (readingClub.Status == ReadingClubStatus.Inactive)
        {
            throw new ConflictException("Reading club is inactive");
        }

        if (readingClub.Status == ReadingClubStatus.Archived)
        {
            throw new ConflictException("Reading club is archived");
        }

        var activeSubscriptionExists = await subscriptionRepository.ExistsActiveAsync(readerId, readingClubId);

        if (activeSubscriptionExists)
        {
            throw new ConflictException("Reader already has an active subscription for this reading club");
        }

        var subscription = new ClubSubscription(readerId, readingClubId);

        await subscriptionRepository.AddAsync(subscription);

        logger.LogInformation("Club subscription {ClubSubscriptionId} created successfully for reader {ReaderId} and reading club {ReadingClubId}",
            subscription.Id,
            readerId,
            readingClubId);

        return subscription;
    }

    public Task<ClubSubscription?> GetByIdAsync(Guid id)
    {
        return subscriptionRepository.GetByIdAsync(id);
    }

    public async Task CancelAsync(Guid id)
    {
        logger.LogInformation("Canceling club subscription {ClubSubscriptionId}", id);

        var subscription = await subscriptionRepository.GetByIdAsync(id);

        if (subscription is null)
        {
            throw new NotFoundException("Club subscription not found");
        }

        subscription.Cancel();

        await subscriptionRepository.UpdateAsync(subscription);

        logger.LogInformation("Club subscription {ClubSubscriptionId} canceled successfully", id);
    }
}

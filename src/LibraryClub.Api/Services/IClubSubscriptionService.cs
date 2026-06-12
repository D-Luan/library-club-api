using LibraryClub.Api.Models;

namespace LibraryClub.Api.Services;

public interface IClubSubscriptionService
{
    Task<ClubSubscription> CreateAsync(Guid readerId, Guid readingClubId);
    Task<ClubSubscription?> GetByIdAsync(Guid id);
    Task CancelAsync(Guid id);
}

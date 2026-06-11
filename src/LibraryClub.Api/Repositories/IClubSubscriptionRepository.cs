using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IClubSubscriptionRepository
{
    Task AddAsync(ClubSubscription subscription);
    Task<ClubSubscription?> GetByIdAsync(Guid id);
    Task<bool> ExistsActiveAsync(Guid readerId, Guid readingClubId);
    Task UpdateAsync(ClubSubscription subscription);
}

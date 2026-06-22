using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IClubSubscriptionRepository
{
    Task AddAsync(ClubSubscription subscription);
    Task<ClubSubscription?> GetByIdAsync(Guid id);
    Task<PagedResult<ClubSubscription>> GetByReaderAsync(Guid readerId, int page, int pageSize);
    Task<PagedResult<ClubSubscription>> GetByReadingClubAsync(Guid readingClubId, int page, int pageSize);
    Task<bool> ExistsActiveAsync(Guid readerId, Guid readingClubId);
    Task UpdateAsync(ClubSubscription subscription);
}

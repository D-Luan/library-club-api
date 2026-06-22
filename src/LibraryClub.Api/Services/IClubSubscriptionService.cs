using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Services;

public interface IClubSubscriptionService
{
    Task<ClubSubscription> CreateAsync(Guid readerId, Guid readingClubId);
    Task<ClubSubscription?> GetByIdAsync(Guid id);
    Task<PagedResult<ClubSubscription>> GetByReaderAsync(Guid readerId, int page, int pageSize);
    Task<PagedResult<ClubSubscription>> GetByReadingClubAsync(Guid readingClubId, int page, int pageSize);
    Task CancelAsync(Guid id);
}

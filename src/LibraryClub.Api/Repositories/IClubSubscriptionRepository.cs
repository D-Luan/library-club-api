using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IClubSubscriptionRepository
{
    Task AddAsync(ClubSubscription subscription, CancellationToken cancellationToken = default);
    Task<ClubSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ClubSubscription>> GetByReaderAsync(
        Guid readerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<PagedResult<ClubSubscription>> GetByReadingClubAsync(
        Guid readingClubId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(
        Guid readerId,
        Guid readingClubId,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(ClubSubscription subscription, CancellationToken cancellationToken = default);
}

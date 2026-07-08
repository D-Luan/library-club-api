using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Services;

public interface IClubSubscriptionService
{
    Task<ClubSubscription> CreateAsync(
        Guid readerId,
        Guid readingClubId,
        CancellationToken cancellationToken = default);

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

    Task CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
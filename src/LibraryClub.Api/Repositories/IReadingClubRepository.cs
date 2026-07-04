using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IReadingClubRepository
{
    Task AddAsync(ReadingClub readingClub, CancellationToken cancellationToken = default);
    Task<ReadingClub?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ReadingClub>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(ReadingClub readingClub, CancellationToken cancellationToken = default);
}

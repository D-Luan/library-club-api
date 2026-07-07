using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Services;

public interface IReadingClubService
{
    Task<ReadingClub> CreateAsync(
        string name,
        string? description,
        string genre,
        CancellationToken cancellationToken = default);

    Task<ReadingClub?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<ReadingClub>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        string name,
        string? description,
        string genre,
        CancellationToken cancellationToken = default);

    Task InactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task ReactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
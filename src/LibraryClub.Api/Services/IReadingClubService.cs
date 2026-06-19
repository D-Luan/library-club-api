using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Services;

public interface IReadingClubService
{
    Task<ReadingClub> CreateAsync(string name, string? description, string genre);
    Task<ReadingClub?> GetByIdAsync(Guid id);
    Task<PagedResult<ReadingClub>> GetPagedAsync(int page, int pageSize);
    Task InactivateAsync(Guid id);
    Task ArchiveAsync(Guid id);
}

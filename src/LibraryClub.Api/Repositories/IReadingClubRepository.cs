using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IReadingClubRepository
{
    Task AddAsync(ReadingClub readingClub);
    Task<ReadingClub?> GetByIdAsync(Guid id);
    Task<PagedResult<ReadingClub>> GetPagedAsync(int page, int pageSize);
    Task UpdateAsync(ReadingClub readingClub);
}

using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IReadingClubRepository
{
    Task AddAsync(ReadingClub readingClub);
    Task<ReadingClub?> GetByIdAsync(Guid id);
    Task UpdateAsync(ReadingClub readingClub);
}

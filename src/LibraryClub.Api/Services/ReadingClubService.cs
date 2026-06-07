using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;

namespace LibraryClub.Api.Services;

public class ReadingClubService(IReadingClubRepository readingClubRepository) : IReadingClubService
{
    public async Task<ReadingClub> CreateAsync(string name, string? description, string genre)
    {
        var readingClub = new ReadingClub(name, description, genre);

        await readingClubRepository.AddAsync(readingClub);

        return readingClub;
    }

    public Task<ReadingClub?> GetByIdAsync(Guid id)
    {
        return readingClubRepository.GetByIdAsync(id);
    }

    public async Task InactivateAsync(Guid id)
    {
        var readingClub = await readingClubRepository.GetByIdAsync(id);

        if (readingClub is null) throw new NotFoundException("Reading club not found");

        readingClub.Inactivate();

        await readingClubRepository.UpdateAsync(readingClub);
    }

    public async Task ArchiveAsync(Guid id)
    {
        var readingClub = await readingClubRepository.GetByIdAsync(id);

        if (readingClub is null) throw new NotFoundException("Reading club not found");

        readingClub.Archive();

        await readingClubRepository.UpdateAsync(readingClub);
    }
}
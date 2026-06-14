using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;

namespace LibraryClub.Api.Services;

public class ReadingClubService(
    IReadingClubRepository readingClubRepository,
    ILogger<ReadingClubService> logger) : IReadingClubService
{
    public async Task<ReadingClub> CreateAsync(string name, string? description, string genre)
    {
        logger.LogInformation("Creating reading club");

        var readingClub = new ReadingClub(name, description, genre);

        await readingClubRepository.AddAsync(readingClub);

        logger.LogInformation("Reading club {ReadingClubId} created successfully", readingClub.Id);

        return readingClub;
    }

    public Task<ReadingClub?> GetByIdAsync(Guid id)
    {
        return readingClubRepository.GetByIdAsync(id);
    }

    public async Task InactivateAsync(Guid id)
    {
        logger.LogInformation("Inactivating reading club {ReadingClubId}", id);

        var readingClub = await readingClubRepository.GetByIdAsync(id);

        if (readingClub is null)
        {
            throw new NotFoundException("Reading club not found");
        }

        readingClub.Inactivate();

        await readingClubRepository.UpdateAsync(readingClub);

        logger.LogInformation("Reading club {ReadingClubId} inactivated successfully", id);
    }

    public async Task ArchiveAsync(Guid id)
    {
        logger.LogInformation("Archiving reading club {ReadingClubId}", id);

        var readingClub = await readingClubRepository.GetByIdAsync(id);

        if (readingClub is null)
        {
            throw new NotFoundException("Reading club not found");
        }

        readingClub.Archive();

        await readingClubRepository.UpdateAsync(readingClub);

        logger.LogInformation("Reading club {ReadingClubId} archived successfully", id);
    }
}

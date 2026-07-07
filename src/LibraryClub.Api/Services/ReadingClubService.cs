using LibraryClub.Api.Common;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;

namespace LibraryClub.Api.Services;

public sealed class ReadingClubService(
    IReadingClubRepository readingClubRepository,
    ILogger<ReadingClubService> logger) : IReadingClubService
{
    public async Task<ReadingClub> CreateAsync(
        string name,
        string? description,
        string genre,
        CancellationToken cancellationToken = default)
    {
        var readingClub = new ReadingClub(name, description, genre);

        await readingClubRepository.AddAsync(readingClub, cancellationToken);

        logger.LogInformation("Reading club {ReadingClubId} created successfully", readingClub.Id);

        return readingClub;
    }

    public Task<ReadingClub?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return readingClubRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<PagedResult<ReadingClub>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Listing reading clubs page {Page} pageSize {PageSize}",
            page,
            pageSize);

        return readingClubRepository.GetPagedAsync(page, pageSize, cancellationToken);
    }

    public async Task UpdateAsync(
        Guid id,
        string name,
        string? description,
        string genre,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating reading club {ReadingClubId}", id);

        var readingClub = await GetRequiredByIdAsync(id, cancellationToken);

        readingClub.UpdateDetails(name, description, genre);

        await readingClubRepository.UpdateAsync(readingClub, cancellationToken);

        logger.LogInformation("Reading club {ReadingClubId} updated successfully", id);
    }

    public async Task InactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Inactivating reading club {ReadingClubId}", id);

        var readingClub = await GetRequiredByIdAsync(id, cancellationToken);

        readingClub.Inactivate();

        await readingClubRepository.UpdateAsync(readingClub, cancellationToken);

        logger.LogInformation("Reading club {ReadingClubId} inactivated successfully", id);
    }

    public async Task ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Reactivating reading club {ReadingClubId}", id);

        var readingClub = await GetRequiredByIdAsync(id, cancellationToken);

        readingClub.Reactivate();

        await readingClubRepository.UpdateAsync(readingClub, cancellationToken);

        logger.LogInformation("Reading club {ReadingClubId} reactivated successfully", id);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Archiving reading club {ReadingClubId}", id);

        var readingClub = await GetRequiredByIdAsync(id, cancellationToken);

        readingClub.Archive();

        await readingClubRepository.UpdateAsync(readingClub, cancellationToken);

        logger.LogInformation("Reading club {ReadingClubId} archived successfully", id);
    }

    private async Task<ReadingClub> GetRequiredByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var readingClub = await readingClubRepository.GetByIdAsync(id, cancellationToken);

        if (readingClub is null)
        {
            throw new NotFoundException("Reading club not found");
        }

        return readingClub;
    }
}
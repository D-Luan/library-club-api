using LibraryClub.Api.Common;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;

namespace LibraryClub.Api.Services;

public sealed class ReaderService(
    IReaderRepository readerRepository,
    ILogger<ReaderService> logger) : IReaderService
{
    public async Task<Reader> CreateAsync(
        string name,
        string email,
        CancellationToken cancellationToken = default)
    {
        var emailAlreadyExists = await readerRepository.ExistsByEmailAsync(
            email,
            cancellationToken);

        if (emailAlreadyExists)
        {
            throw new ConflictException("Reader email already exists");
        }

        var reader = new Reader(name, email);

        await readerRepository.AddAsync(reader, cancellationToken);

        logger.LogInformation("Reader {ReaderId} created successfully", reader.Id);

        return reader;
    }

    public Task<Reader?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return readerRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<PagedResult<Reader>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Listing readers page {Page} pageSize {PageSize}",
            page,
            pageSize);

        return readerRepository.GetPagedAsync(page, pageSize, cancellationToken);
    }

    public async Task InactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Inactivating reader {ReaderId}", id);

        var reader = await readerRepository.GetByIdAsync(id, cancellationToken);

        if (reader is null)
        {
            throw new NotFoundException("Reader not found");
        }

        reader.Inactivate();

        await readerRepository.UpdateAsync(reader, cancellationToken);

        logger.LogInformation("Reader {ReaderId} inactivated successfully", id);
    }

    public async Task ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Reactivating reader {ReaderId}", id);

        var reader = await readerRepository.GetByIdAsync(id, cancellationToken);

        if (reader is null)
        {
            throw new NotFoundException("Reader not found");
        }

        reader.Reactivate();

        await readerRepository.UpdateAsync(reader, cancellationToken);

        logger.LogInformation("Reader {ReaderId} reactivated successfully", id);
    }
}

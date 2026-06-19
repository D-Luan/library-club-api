using LibraryClub.Api.Common;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;

namespace LibraryClub.Api.Services;

public class ReaderService(
    IReaderRepository readerRepository,
    ILogger<ReaderService> logger) : IReaderService
{
    public async Task<Reader> CreateAsync(string name, string email)
    {
        logger.LogInformation("Creating reader...");

        var emailAlreadyExists = await readerRepository.ExistsByEmailAsync(email);

        if (emailAlreadyExists)
        {
            throw new ConflictException("Reader email already exists");
        }

        var reader = new Reader(name, email);

        await readerRepository.AddAsync(reader);

        logger.LogInformation("Reader {ReaderId} created successfully", reader.Id);

        return reader;
    }

    public Task<Reader?> GetByIdAsync(Guid id)
    {
        return readerRepository.GetByIdAsync(id);
    }

    public async Task InactivateAsync(Guid id)
    {
        logger.LogInformation("Inactivating reader {ReaderId}", id);

        var reader = await readerRepository.GetByIdAsync(id);

        if (reader is null)
        {
            throw new NotFoundException("Reader not found");
        }

        reader.Inactivate();

        await readerRepository.UpdateAsync(reader);

        logger.LogInformation("Reader {ReaderId} inactivated successfully", id);
    }

    public async Task<PagedResult<Reader>> GetPagedAsync(int page, int pageSize)
    {
        logger.LogInformation("Listing readers page {Page} pageSize {PageSize}", page, pageSize);

        return await readerRepository.GetPagedAsync(page, pageSize);
    }
}

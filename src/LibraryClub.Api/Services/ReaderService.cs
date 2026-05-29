using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;

namespace LibraryClub.Api.Services;

public class ReaderService(IReaderRepository readerRepository) : IReaderService
{
    public async Task<Reader> CreateAsync(string name, string email)
    {
        var emailAlreadyExists = await readerRepository.ExistsByEmailAsync(email);

        if (emailAlreadyExists)
        {
            throw new ConflictException("Reader email already exists");
        }

        var reader = new Reader(name, email);

        await readerRepository.AddAsync(reader);

        return reader;
    }

    public Task<Reader?> GetByIdAsync(Guid id)
    {
        return readerRepository.GetByIdAsync(id);
    }

    public async Task InactivateAsync(Guid id)
    {
        var reader = await readerRepository.GetByIdAsync(id);

        if (reader is null)
        {
            throw new NotFoundException("Reader not found");
        }

        reader.Inactivate();

        await readerRepository.UpdateAsync(reader);
    }
}
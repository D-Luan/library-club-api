using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IReaderRepository
{
    Task AddAsync(Reader reader);
    Task<Reader?> GetByIdAsync(Guid id);
    Task<Reader?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task UpdateAsync(Reader reader);
}

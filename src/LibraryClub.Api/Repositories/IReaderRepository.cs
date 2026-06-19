using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IReaderRepository
{
    Task AddAsync(Reader reader);
    Task<Reader?> GetByIdAsync(Guid id);
    Task<Reader?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<PagedResult<Reader>> GetPagedAsync(int page, int pageSize);
    Task UpdateAsync(Reader reader);
}

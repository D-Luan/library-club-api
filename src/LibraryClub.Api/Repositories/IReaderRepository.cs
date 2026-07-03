using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Repositories;

public interface IReaderRepository
{
    Task AddAsync(Reader reader, CancellationToken cancellationToken = default);
    Task<Reader?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Reader?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<PagedResult<Reader>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Reader reader, CancellationToken cancellationToken = default);
}

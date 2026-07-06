using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Services;

public interface IReaderService
{
    Task<Reader> CreateAsync(
        string name,
        string email,
        CancellationToken cancellationToken = default);

    Task<Reader?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Reader>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task InactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

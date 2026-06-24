using LibraryClub.Api.Common;
using LibraryClub.Api.Models;

namespace LibraryClub.Api.Services;

public interface IReaderService
{
    Task<Reader> CreateAsync(string name, string email);
    Task<Reader?> GetByIdAsync(Guid id);
    Task<PagedResult<Reader>> GetPagedAsync(int page, int pageSize);
    Task InactivateAsync(Guid id);
    Task ReactivateAsync(Guid id);
}

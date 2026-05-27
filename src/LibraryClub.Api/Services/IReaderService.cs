using LibraryClub.Api.Models;

namespace LibraryClub.Api.Services;

public interface IReaderService
{
    Task<Reader> CreateAsync(string name, string email);
    Task<Reader?> GetByIdAsync(Guid id);
    Task InactivateAsync(Guid id);
}

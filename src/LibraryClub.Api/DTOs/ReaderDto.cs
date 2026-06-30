namespace LibraryClub.Api.DTOs;

public record CreateReaderRequest(
    string Name, 
    string Email);

public record ReaderResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    DateTime CreatedAt);

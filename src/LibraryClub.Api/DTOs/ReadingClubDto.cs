namespace LibraryClub.Api.DTOs;

public record CreateReadingClubRequest(
    string Name,
    string? Description,
    string Genre
);

public record UpdateReadingClubRequest(
    string Name,
    string? Description,
    string Genre
);

public record ReadingClubResponse(
    Guid Id,
    string Name,
    string? Description,
    string Genre,
    string Status,
    DateTime CreatedAt
);

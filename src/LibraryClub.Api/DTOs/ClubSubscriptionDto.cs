namespace LibraryClub.Api.DTOs;

public record CreateClubSubscriptionRequest(
    Guid ReaderId,
    Guid ReadingClubId
);

public record ClubSubscriptionResponse(
    Guid Id,
    Guid ReaderId,
    Guid ReadingClubId,
    string Status,
    DateTime CreatedAt,
    DateTime? CanceledAt
);

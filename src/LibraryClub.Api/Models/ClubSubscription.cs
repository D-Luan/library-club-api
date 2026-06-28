using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LibraryClub.Api.Models;

/// <summary>
/// Represents a reader subscription to a reading club.
/// Canceled subscriptions are preserved as history instead of being deleted.
/// </summary>
public class ClubSubscription
{
    public Guid Id { get; private set; }
    public Guid ReaderId { get; private set; }
    public Guid ReadingClubId { get; private set; }
    public ClubSubscriptionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }

    public ClubSubscription(Guid readerId, Guid readingClubId)
    {
        Id = Guid.NewGuid();
        Status = ClubSubscriptionStatus.Active;
        CreatedAt = DateTime.UtcNow;

        SetReaderId(readerId);
        SetReadingClubId(readingClubId);
    }

    private ClubSubscription(
        Guid id,
        Guid readerId,
        Guid readingClubId,
        ClubSubscriptionStatus status,
        DateTime createdAt,
        DateTime? canceledAt)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException("Club subscription id cannot be empty");
        }

        if (createdAt == default)
        {
            throw new DomainValidationException("Club subscription creation date cannot be empty");
        }

        if (!Enum.IsDefined(status))
        {
            throw new DomainValidationException("Invalid club subscription status");
        }

        if (status == ClubSubscriptionStatus.Active && canceledAt is not null)
        {
            throw new DomainValidationException("Active club subscription cannot have cancellation date");
        }

        if (status == ClubSubscriptionStatus.Canceled && canceledAt is null)
        {
            throw new DomainValidationException("Canceled club subscription must have cancellation date");
        }

        if (canceledAt.HasValue)
        {
            var cancellationDate = canceledAt.Value;


            if (cancellationDate == default)
            {
                throw new DomainValidationException("Club subscription cancellation date cannot be empty");
            }

            if (cancellationDate < createdAt)
            {
                throw new DomainValidationException("Club subscription cancellation date cannot be earlier than creation date");
            }
        }

        Id = id;
        Status = status;
        CreatedAt = createdAt;
        CanceledAt = canceledAt;

        SetReaderId(readerId);
        SetReadingClubId(readingClubId);
    }

    /// <summary>
    /// Rebuilds a club subscription loaded from persistence without creating a new identity.
    /// Use this method only when mapping database records back to the domain model.
    /// </summary>
    public static ClubSubscription Restore(
        Guid id,
        Guid readerId,
        Guid readingClubId,
        ClubSubscriptionStatus status,
        DateTime createdAt,
        DateTime? canceledAt)
    {
        return new ClubSubscription(
            id, 
            readerId, 
            readingClubId, 
            status, 
            createdAt, 
            canceledAt);
    }

    /// <summary>
    /// Cancels an active subscription and records when the cancellation happened.
    /// Already canceled subscriptions cannot be canceled again.
    /// </summary>
    public void Cancel()
    {
        if (Status == ClubSubscriptionStatus.Canceled)
        {
            throw new ConflictException("Club subscription is already canceled");
        }

        Status = ClubSubscriptionStatus.Canceled;
        CanceledAt = DateTime.UtcNow;
    }

    private void SetReaderId(Guid readerId)
    {
        if (readerId == Guid.Empty)
        {
            throw new DomainValidationException("Reader id cannot be empty");
        }

        ReaderId = readerId;
    }

    private void SetReadingClubId(Guid readingClubId)
    {
        if (readingClubId == Guid.Empty)
        {
            throw new DomainValidationException("Reading club id cannot be empty");
        }

        ReadingClubId = readingClubId;
    }
}

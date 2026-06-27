using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;

namespace LibraryClub.Api.Models;

/// <summary>
/// Represents a reading club and centralizes rules for club details and status transitions.
/// Archived clubs are historical records and cannot be edited, inactivated, or reactivated.
/// </summary>
public class ReadingClub
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Genre { get; private set; } = string.Empty;
    public ReadingClubStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ReadingClub(string name, string? description, string genre)
    {
        Id = Guid.NewGuid();
        Status = ReadingClubStatus.Active;
        CreatedAt = DateTime.UtcNow;

        SetName(name);
        SetDescription(description);
        SetGenre(genre);
    }

    private ReadingClub(
        Guid id,
        string name,
        string? description,
        string genre,
        ReadingClubStatus status,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException("Reading club id cannot be empty");
        }

        if (createdAt == default)
        {
            throw new DomainValidationException("Reading club creation date cannot be empty");
        }

        Id = id;
        Status = status;
        CreatedAt = createdAt;

        SetName(name);
        SetDescription(description);
        SetGenre(genre);
    }

    /// <summary>
    /// Rebuilds a reading club loaded from persistence without creating a new identity.
    /// Use this method only when mapping database records back to the domain model.
    /// </summary>
    public static ReadingClub Restore(
        Guid id,
        string name,
        string? description,
        string genre,
        ReadingClubStatus status,
        DateTime createdAt)
    {
        return new ReadingClub(id, name, description, genre, status, createdAt);
    }

    /// <summary>
    /// Updates editable club details without changing the current status.
    /// Active and inactive clubs can be edited; archived clubs cannot.
    /// </summary>
    public void UpdateDetails(string name, string? description, string genre)
    {
        if (Status == ReadingClubStatus.Archived)
        {
            throw new ConflictException("Archived reading club cannot be updated");
        }

        SetName(name);
        SetDescription(description);
        SetGenre(genre);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Name cannot be empty");
        }

        var trimmedName = name.Trim();

        if (trimmedName.Length > 150)
        {
            throw new DomainValidationException("Name must have at most 150 characters");
        }

        Name = trimmedName;
    }

    private void SetDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
            return;
        }

        var trimmedDescription = description.Trim();

        if (trimmedDescription.Length > 1000)
        {
            throw new DomainValidationException("Description must have at most 1000 characters");
        }

        Description = trimmedDescription;
    }

    private void SetGenre(string genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
        {
            throw new DomainValidationException("Genre cannot be empty");
        }

        var trimmedGenre = genre.Trim();

        if (trimmedGenre.Length > 100)
        {
            throw new DomainValidationException("Genre must have at most 100 characters");
        }

        Genre = trimmedGenre;
    }

    /// <summary>
    /// Inactivates the reading club.
    /// Inactive clubs cannot be inactivated again, and archived clubs cannot be inactivated.
    /// </summary>
    public void Inactivate()
    {
        if (Status == ReadingClubStatus.Inactive)
        {
            throw new ConflictException("Reading club is already inactive");
        }

        if (Status == ReadingClubStatus.Archived)
        {
            throw new ConflictException("Archived reading club cannot be inactivated");
        }

        Status = ReadingClubStatus.Inactive;
    }

    /// <summary>
    /// Archives the reading club as a historical record.
    /// Archived clubs cannot be archived again.
    /// </summary>
    public void Archive()
    {
        if (Status == ReadingClubStatus.Archived)
        {
            throw new ConflictException("Reading club is already archived");
        }

        Status = ReadingClubStatus.Archived;
    }

    /// <summary>
    /// Reactivates an inactive reading club.
    /// Active clubs and archived clubs cannot be reactivated.
    /// </summary>
    public void Reactivate()
    {
        if (Status == ReadingClubStatus.Active)
        {
            throw new ConflictException("Reading club is already active");
        }

        if (Status == ReadingClubStatus.Archived)
        {
            throw new ConflictException("Archived reading club cannot be reactivated");
        }

        Status = ReadingClubStatus.Active;
    }
}

using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;

namespace LibraryClub.Api.Models;

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

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Name cannot be empty");
        }

        if (name.Length > 150)
        {
            throw new DomainValidationException("Name must have at most 150 characters");
        }

        Name = name.Trim();
    }

    public void SetDescription(string? description)
    {
        if (description is not null && description.Length > 1000)
        {
            throw new DomainValidationException("Description must have at most 1000 characters");
        }

        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void SetGenre(string genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
        {
            throw new DomainValidationException("Genre cannot be empty");
        }

        if (genre.Length > 100)
        {
            throw new DomainValidationException("Genre must have at most 100 characters");
        }

        Genre = genre.Trim();
    }

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

    public void Archive()
    {
        if (Status == ReadingClubStatus.Archived)
        {
            throw new ConflictException("Reading club is already archived");
        }

        Status = ReadingClubStatus.Archived;
    }

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

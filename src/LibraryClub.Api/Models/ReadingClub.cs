using LibraryClub.Api.Enums;

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
            throw new ArgumentException("Reading club id cannot be empty");
        }

        if (createdAt == default)
        {
            throw new ArgumentException("Reading club creation date cannot be empty");
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
            throw new ArgumentException("Name cannot be empty");
        }

        if (name.Length > 150)
        {
            throw new ArgumentException("Name must have at most 150 characters");
        }

        Name = name.Trim();
    }

    public void SetDescription(string? description)
    {
        if (description is not null && description.Length > 1000)
        {
            throw new ArgumentException("Description must have at most 1000 characters");
        }

        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void SetGenre(string genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
        {
            throw new ArgumentException("Genre cannot be empty");
        }

        if (genre.Length > 100)
        {
            throw new ArgumentException("Genre must have at most 100 characters");
        }

        Genre = genre.Trim();
    }

    public void Inactivate()
    {
        if (Status == ReadingClubStatus.Inactive)
        {
            throw new InvalidOperationException("Reading club is already inactive");
        }

        if (Status == ReadingClubStatus.Archived)
        {
            throw new InvalidOperationException("Archived reading club cannot be inactivated");
        }

        Status = ReadingClubStatus.Inactive;
    }

    public void Archive()
    {
        if (Status == ReadingClubStatus.Archived)
        {
            throw new InvalidOperationException("Reading club is already archived");
        }

        Status = ReadingClubStatus.Archived;
    }
}

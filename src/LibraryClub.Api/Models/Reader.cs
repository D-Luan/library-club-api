using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using System.Text.RegularExpressions;

namespace LibraryClub.Api.Models;

/// <summary>
/// Represents a reader registered in the library club platform.
/// The entity owns its status transitions and protects reader-related business rules.
/// </summary>
public class Reader
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public ReaderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Reader(string name, string email)
    {
        Id = Guid.NewGuid();
        Status = ReaderStatus.Active;
        CreatedAt = DateTime.UtcNow;

        ChangeName(name);
        ChangeEmail(email);
    }

    private Reader(
        Guid id, 
        string name, 
        string email, 
        ReaderStatus status, 
        DateTime createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException("Reader id cannot be empty");
        }

        if (createdAt == default)
        {
            throw new DomainValidationException("Reader creation date cannot be empty");
        }

        if (!Enum.IsDefined(status))
        {
            throw new DomainValidationException("Invalid reader status");
        }

        Id = id;
        Status = status;
        CreatedAt = createdAt;

        ChangeName(name);
        ChangeEmail(email);
    }

    /// <summary>
    /// Rebuilds a reader loaded from persistence without creating a new identity.
    /// Use this method only when mapping database records back to the domain model.
    /// </summary>
    public static Reader Restore(
        Guid id,
        string name,
        string email,
        ReaderStatus status,
        DateTime createdAt)
    {
        return new Reader(id, name, email, status, createdAt);
    }

    public void ChangeName(string name)
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

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException("Email cannot be empty");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > 255)
        {
            throw new DomainValidationException("Email must have at most 255 characters");
        }

        if (!Regex.IsMatch(normalizedEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new DomainValidationException("Invalid email format");
        }

        Email = normalizedEmail;
    }

    /// <summary>
    /// Inactivates the reader.
    /// Readers that are already inactive cannot be inactivated again.
    /// </summary>
    public void Inactivate()
    {
        if (Status == ReaderStatus.Inactive)
        {
            throw new ConflictException("Reader is already inactive");
        }

        Status = ReaderStatus.Inactive;
    }

    /// <summary>
    /// Reactivates an inactive reader.
    /// Active readers cannot be reactivated.
    /// </summary>
    public void Reactivate()
    {
        if (Status == ReaderStatus.Active)
        {
            throw new ConflictException("Reader is already active");
        }

        Status = ReaderStatus.Active;
    }
}

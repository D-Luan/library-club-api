using LibraryClub.Api.Enums;
using System.Text.RegularExpressions;

namespace LibraryClub.Api.Models;

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

        SetName(name);
        SetEmail(email);
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
            throw new ArgumentException("Reader id cannot be empty");
        }

        if (createdAt == default)
        {
            throw new ArgumentException("Reader creation date cannot be empty");
        }

        Id = id;
        Status = status;
        CreatedAt = createdAt;

        SetName(name);
        SetEmail(email);
    }

    public static Reader Restore(
        Guid id, 
        string name, 
        string email, 
        ReaderStatus status, 
        DateTime createdAt)
    {
        return new Reader(id, name, email, status, createdAt);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty");
        }

        Name = name.Trim();
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty");
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new ArgumentException("Invalid email format");
        }

        Email = email.Trim().ToLowerInvariant();
    }

    public void Inactivate()
    {
        if (Status == ReaderStatus.Inactive)
        {
            throw new InvalidOperationException("Reader is already inactive");
        }

        Status = ReaderStatus.Inactive;
    }
}

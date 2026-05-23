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
        CreatedAt = DateTime.UtcNow;
        Status = ReaderStatus.Active;

        SetName(name);
        SetEmail(email);
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

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^s@\s]+$"))
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

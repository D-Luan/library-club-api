using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;

namespace LibraryClub.Tests.UnitTests;

[Trait("Category", "Unit")]
public class ReaderTests
{
    [Fact]
    public void Constructor_ShouldCreateReader_WhenDataIsValid()
    {
        var reader = new Reader("Clara Bennett", "clara.bennett@email.com");

        Assert.NotEqual(Guid.Empty, reader.Id);
        Assert.Equal("Clara Bennett", reader.Name);
        Assert.Equal("clara.bennett@email.com", reader.Email);
        Assert.Equal(ReaderStatus.Active, reader.Status);
        Assert.NotEqual(default, reader.CreatedAt);
    }

    [Fact]
    public void Constructor_ShouldTrimNameAndNormalizeEmail()
    {
        var reader = new Reader("  Ana Martins  ", "  ANA.MARTINS@EMAIL.COM  ");

        Assert.Equal("Ana Martins", reader.Name);
        Assert.Equal("ana.martins@email.com", reader.Email);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Reader("", "nora.wells@email.com"));

        Assert.Equal("Name cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsTooLong()
    {
        var name = new string('A', 151);

        var exception = Assert.Throws<DomainValidationException>(() =>
            new Reader(name, "reader@email.com"));

        Assert.Equal("Name must have at most 150 characters", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEmailIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Reader("Mateo Costa", ""));

        Assert.Equal("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEmailIsInvalid()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Reader("Olivia Parker", "invalid-email"));

        Assert.Equal("Invalid email format", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEmailIsTooLong()
    {
        var email = $"{new string('a', 246)}@email.com";

        var exception = Assert.Throws<DomainValidationException>(() =>
            new Reader("Ana Martins", email));

        Assert.Equal("Email must have at most 255 characters", exception.Message);
    }

    [Fact]
    public void Restore_ShouldCreateReader_WhenDataIsValid()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);

        var reader = Reader.Restore(
            id,
            "Helena Brooks",
            "helena.brooks@email.com",
            ReaderStatus.Active,
            createdAt);

        Assert.Equal(id, reader.Id);
        Assert.Equal("Helena Brooks", reader.Name);
        Assert.Equal("helena.brooks@email.com", reader.Email);
        Assert.Equal(ReaderStatus.Active, reader.Status);
        Assert.Equal(createdAt, reader.CreatedAt);
    }

    [Fact]
    public void Restore_ShouldTrimNameAndNormalizeEmail()
    {
        var reader = Reader.Restore(
            Guid.NewGuid(),
            "  Ana Martins  ",
            "  ANA.MARTINS@EMAIL.COM  ",
            ReaderStatus.Active,
            DateTime.UtcNow);

        Assert.Equal("Ana Martins", reader.Name);
        Assert.Equal("ana.martins@email.com", reader.Email);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenIdIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            Reader.Restore(
                Guid.Empty,
                "Vitor Almeida",
                "vitor.almeida@email.com",
                ReaderStatus.Active,
                DateTime.UtcNow));

        Assert.Equal("Reader id cannot be empty", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenCreatedAtIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            Reader.Restore(
                Guid.NewGuid(),
                "Laura Mendes",
                "laura.mendes@email.com",
                ReaderStatus.Active,
                default));

        Assert.Equal("Reader creation date cannot be empty", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenStatusIsInvalid()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            Reader.Restore(
                Guid.NewGuid(),
                "Diego Santos",
                "diego.santos@email.com",
                (ReaderStatus)999,
                DateTime.UtcNow));

        Assert.Equal("Invalid reader status", exception.Message);
    }

    [Fact]
    public void Inactivate_ShouldSetStatusToInactive_WhenReaderIsActive()
    {
        var reader = new Reader("Robert Smith", "robert.smith@email.com");

        reader.Inactivate();

        Assert.Equal(ReaderStatus.Inactive, reader.Status);
    }

    [Fact]
    public void Inactivate_ShouldThrowException_WhenReaderIsAlreadyInactive()
    {
        var reader = new Reader("Mary Taylor", "mary@gmail.com");
        reader.Inactivate();

        var exception = Assert.Throws<ConflictException>(reader.Inactivate);

        Assert.Equal("Reader is already inactive", exception.Message);
    }

    [Fact]
    public void Reactivate_ShouldSetStatusToActive_WhenReaderIsInactive()
    {
        var reader = new Reader("Isabela Martins", "isabela.martins@email.com");
        reader.Inactivate();

        reader.Reactivate();

        Assert.Equal(ReaderStatus.Active, reader.Status);
    }

    [Fact]
    public void Reactivate_ShouldThrowConflict_WhenReaderIsAlreadyActive()
    {
        var reader = new Reader("Gabriel Oliveira", "gabriel.oliveira@email.com");

        var exception = Assert.Throws<ConflictException>(reader.Reactivate);

        Assert.Equal("Reader is already active", exception.Message);
    }
}

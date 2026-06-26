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
        var reader = new Reader("John Doe", "john.doe@email.com");

        Assert.NotEqual(Guid.Empty, reader.Id);
        Assert.Equal("John Doe", reader.Name);
        Assert.Equal("john.doe@email.com", reader.Email);
        Assert.Equal(ReaderStatus.Active, reader.Status);
        Assert.NotEqual(default, reader.CreatedAt);
    }

    [Fact]
    public void ChangeName_ShouldThrowException_WhenNameIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() => 
            new Reader("", "john.doe@email.com"));

        Assert.Equal("Name cannot be empty", exception.Message);
    }

    [Fact]
    public void ChangeEmail_ShouldThrowException_WhenEmailIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() => 
            new Reader("John Doe", ""));

        Assert.Equal("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void ChangeEmail_ShouldThrowException_WhenEmailIsInvalid()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Reader("John Doe", "invalid-email"));

        Assert.Equal("Invalid email format", exception.Message);
    }

    [Fact]
    public void Inactivate_ShouldSetStatusToInactivate_WhenReaderIsActive()
    {
        var reader = new Reader("Robert Smith", "robert.smith@email.com");

        reader.Inactivate();

        Assert.Equal(ReaderStatus.Inactive, reader.Status);
    }

    [Fact]
    public void Inactivate_ShouldThrowException_WhenReaderIsAlreadyInactivate()
    {
        var reader = new Reader("Mary Taylor", "mary@gmail.com");
        reader.Inactivate();

        var exception = Assert.Throws<ConflictException>(() =>
            reader.Inactivate());

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
    public void Reactivate_ShouldThrowConflictException_WhenReaderIsAlreadyActive()
    {
        var reader = new Reader("Gabriel Oliveira", "gabriel.oliveira@email.com");

        var exception = Assert.Throws<ConflictException>(() =>
            reader.Reactivate());

        Assert.Equal("Reader is already active", exception.Message);
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
    public void Constructor_ShouldThrowException_WhenEmailIsTooLong()
    {
        var email = $"{new string('a', 246)}@email.com";

        var exception = Assert.Throws<DomainValidationException>(() =>
            new Reader("Ana Martins", email));

        Assert.Equal("Email must have at most 255 characters", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldTrimNameAndNormalizeEmail()
    {
        var reader = new Reader("  Ana Martins  ", "  ANA.MARTINS@EMAIL.COM  ");

        Assert.Equal("Ana Martins", reader.Name);
        Assert.Equal("ana.martins@email.com", reader.Email);
    }
}

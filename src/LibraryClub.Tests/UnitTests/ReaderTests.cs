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
    public void SetName_ShouldThrowException_WhenNameIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() => 
            new Reader("", "john.doe@email.com"));

        Assert.Equal("Name cannot be empty", exception.Message);
    }

    [Fact]
    public void SetEmail_ShouldThrowException_WhenEmailIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() => 
            new Reader("John Doe", ""));

        Assert.Equal("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void SetEmail_ShouldThrowException_WhenEmailIsInvalid()
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
}

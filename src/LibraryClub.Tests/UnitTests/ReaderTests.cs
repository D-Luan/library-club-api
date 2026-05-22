using LibraryClub.Api.Models;

namespace LibraryClub.Tests.UnitTests;

public class ReaderTests
{
    [Fact]
    public void Create_ShouldCreateReader_WhenDataIsValid()
    {
        var reader = new Reader("John Doe", "john.doe@email.com");

        Assert.NotEqual(Guid.Empty, reader.Id);
        Assert.Equal("John Doe", reader.Name);
        Assert.Equal("john.doe@email.com", reader.Email);
        Assert.NotEqual(default, reader.CreatedAt);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNameIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Reader("", "john.doe@email.com"));

        Assert.Equal("Name cannot be empty", exception.Message);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenEmailIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Reader("John Doe", ""));

        Assert.Equal("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenEmailIsInvalid()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Reader("John Doe", "invalid-email"));

        Assert.Equal("Invalid email format", exception.Message);
    }
}

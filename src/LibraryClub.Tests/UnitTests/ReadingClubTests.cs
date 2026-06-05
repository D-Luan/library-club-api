using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;

namespace LibraryClub.Tests.UnitTests;

[Trait("Category", "Unit")]
public class ReadingClubTests
{
    [Fact]
    public void Constructor_ShouldCreateReadingClub_WhenDataIsValid()
    {
        var readingClub = new ReadingClub("Classic Books", "Monthly classics discussion",
"Classics");

        Assert.NotEqual(Guid.Empty, readingClub.Id);
        Assert.Equal("Classic Books", readingClub.Name);
        Assert.Equal("Monthly classics discussion", readingClub.Description);
        Assert.Equal("Classics", readingClub.Genre);
        Assert.Equal(ReadingClubStatus.Active, readingClub.Status);
        Assert.NotEqual(default, readingClub.CreatedAt);
    }

    [Fact]
    public void Constructor_ShouldAllowNullDescription()
    {
        var readingClub = new ReadingClub("Sci-Fi Club", null, "Science Fiction");

        Assert.Null(readingClub.Description);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ReadingClub("", null, "Fantasy"));

        Assert.Equal("Name cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsTooLong()
    {
        var name = new string('A', 151);

        var exception = Assert.Throws<ArgumentException>(() =>
            new ReadingClub(name, null, "Fantasy"));

        Assert.Equal("Name must have at most 150 characters", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDescriptionIsTooLong()
    {
        var description = new string('A', 1001);

        var exception = Assert.Throws<ArgumentException>(() =>
            new ReadingClub("Fantasy Club", description, "Fantasy"));

        Assert.Equal("Description must have at most 1000 characters", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGenreIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ReadingClub("Fantasy Club", null, ""));

        Assert.Equal("Genre cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGenreIsTooLong()
    {
        var genre = new string('A', 101);

        var exception = Assert.Throws<ArgumentException>(() =>
            new ReadingClub("Fantasy Club", null, genre));

        Assert.Equal("Genre must have at most 100 characters", exception.Message);
    }

    [Fact]
    public void Inactivate_ShouldSetStatusToInactive_WhenReadingClubIsActive()
    {
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        readingClub.Inactivate();

        Assert.Equal(ReadingClubStatus.Inactive, readingClub.Status);
    }

    [Fact]
    public void Inactivate_ShouldThrowException_WhenReadingClubIsAlreadyInactive()
    {
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");
        readingClub.Inactivate();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            readingClub.Inactivate());

        Assert.Equal("Reading club is already inactive", exception.Message);
    }

    [Fact]
    public void Inactivate_ShouldThrowException_WhenReadingClubIsArchived()
    {
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");
        readingClub.Archive();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            readingClub.Inactivate());

        Assert.Equal("Archived reading club cannot be inactivated", exception.Message);
    }

    [Fact]
    public void Archive_ShouldSetStatusToArchived_WhenReadingClubIsActive()
    {
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        readingClub.Archive();

        Assert.Equal(ReadingClubStatus.Archived, readingClub.Status);
    }

    [Fact]
    public void Archive_ShouldSetStatusToArchived_WhenReadingClubIsInactive()
    {
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");
        readingClub.Inactivate();

        readingClub.Archive();

        Assert.Equal(ReadingClubStatus.Archived, readingClub.Status);
    }

    [Fact]
    public void Archive_ShouldThrowException_WhenReadingClubIsAlreadyArchived()
    {
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");
        readingClub.Archive();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            readingClub.Archive());

        Assert.Equal("Reading club is already archived", exception.Message);
    }
}
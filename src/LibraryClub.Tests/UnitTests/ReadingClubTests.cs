using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;

namespace LibraryClub.Tests.UnitTests;

[Trait("Category", "Unit")]
public class ReadingClubTests
{
    [Fact]
    public void Constructor_ShouldCreateReadingClub_WhenDataIsValid()
    {
        var readingClub = new ReadingClub("Classic Books", "Monthly classics discussion", "Classics");

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
    public void Constructor_ShouldSetDescriptionToNull_WhenDescriptionIsWhiteSpace()
    {
        var readingClub = new ReadingClub(
            "Science Fiction Society", 
            "   ", 
            "Science Fiction");

        Assert.Null(readingClub.Description);
    }

    [Fact]
    public void Constructor_ShouldTrimNameDescriptionAndGenre()
    {
        var readingClub = new ReadingClub(
            "  Mystery Book Circle  ",
            "  Discussions about mystery novels  ",
            "  Mystery  ");

        Assert.Equal("Mystery Book Circle", readingClub.Name);
        Assert.Equal("Discussions about mystery novels", readingClub.Description);
        Assert.Equal("Mystery", readingClub.Genre);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new ReadingClub("", null, "Fantasy"));

        Assert.Equal("Name cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsTooLong()
    {
        var name = new string('A', 151);

        var exception = Assert.Throws<DomainValidationException>(() =>
            new ReadingClub(name, null, "Fantasy"));

        Assert.Equal("Name must have at most 150 characters", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDescriptionIsTooLong()
    {
        var description = new string('A', 1001);

        var exception = Assert.Throws<DomainValidationException>(() =>
            new ReadingClub("Fantasy Club", description, "Fantasy"));

        Assert.Equal("Description must have at most 1000 characters", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGenreIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new ReadingClub("Fantasy Club", null, ""));

        Assert.Equal("Genre cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGenreIsTooLong()
    {
        var genre = new string('A', 101);

        var exception = Assert.Throws<DomainValidationException>(() =>
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

        var exception = Assert.Throws<ConflictException>(() =>
            readingClub.Inactivate());

        Assert.Equal("Reading club is already inactive", exception.Message);
    }

    [Fact]
    public void Inactivate_ShouldThrowException_WhenReadingClubIsArchived()
    {
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");
        readingClub.Archive();

        var exception = Assert.Throws<ConflictException>(() =>
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

        var exception = Assert.Throws<ConflictException>(() =>
            readingClub.Archive());

        Assert.Equal("Reading club is already archived", exception.Message);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateFields_WhenReadingClubIsActive()
    {
        var readingClub = new ReadingClub("Romance Club", "Romance Books", "Romance");

        var createdAt = readingClub.CreatedAt;

        readingClub.UpdateDetails("Drama Club", "Drama Books", "Drama");

        Assert.Equal("Drama Club", readingClub.Name);
        Assert.Equal("Drama Books", readingClub.Description);
        Assert.Equal("Drama", readingClub.Genre);
        Assert.Equal(ReadingClubStatus.Active, readingClub.Status);
        Assert.Equal(createdAt, readingClub.CreatedAt);
    }

    [Fact]
    public void UpdateDetails_ShouldTrimNameDescriptionAndGenre()
    {
        var readingClub = new ReadingClub(
            "Coastal Classics",
            "Monthly discussions on classic literature",
            "Classics");

        readingClub.UpdateDetails(
            "  Mystery Book Circle  ",
            "  Discussions about mystery novels  ",
            "  Mystery  ");

        Assert.Equal("Mystery Book Circle", readingClub.Name);
        Assert.Equal("Discussions about mystery novels", readingClub.Description);
        Assert.Equal("Mystery", readingClub.Genre);
        Assert.Equal(ReadingClubStatus.Active, readingClub.Status);
    }

    [Fact]
    public void UpdateDetails_ShouldSetDescriptionToNull_WhenDescriptionIsWhiteSpace()
    {
        var readingClub = new ReadingClub(
            "Science Fiction Society",
            "Exploring speculative fiction",
            "Science Fiction");

        readingClub.UpdateDetails(
            "Historical Fiction Forum",
            "   ",
            "Historical Fiction");

        Assert.Equal("Historical Fiction Forum", readingClub.Name);
        Assert.Null(readingClub.Description);
        Assert.Equal("Historical Fiction", readingClub.Genre);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateFields_WhenReadingClubIsInactive()
    {
        var readingClub = new ReadingClub("Science Club", null, "Sci-fi");

        readingClub.Inactivate();

        readingClub.UpdateDetails("Suspense Club", null, "Suspense");

        Assert.Equal("Suspense Club", readingClub.Name);
        Assert.Null(readingClub.Description);
        Assert.Equal("Suspense", readingClub.Genre);
        Assert.Equal(ReadingClubStatus.Inactive, readingClub.Status);
    }

    [Fact]
    public void UpdateDetails_ShouldThrowConflictException_WhenReadingClubIsArchived()
    {
        var readingClub = new ReadingClub("Fantasy Club", "Fantasy books", "Fantasy");

        readingClub.Archive();

        var exception = Assert.Throws<ConflictException>(() =>
            readingClub.UpdateDetails("Science Club", "Science Fiction books", "Sci-fi"));

        Assert.Equal("Archived reading club cannot be updated", exception.Message);
    }

    [Fact]
    public void Reactivate_ShouldSetStatusToActive_WhenReadingClubIsInactive()
    {
        var readingClub = new ReadingClub(
            "Historical Fiction Forum",
            "Discussions about historical novels",
            "Historical Fiction");

        readingClub.Inactivate();

        readingClub.Reactivate();

        Assert.Equal(ReadingClubStatus.Active, readingClub.Status);
    }

    [Fact]
    public void Reactivate_ShouldThrowConflictException_WhenReadingClubIsAlreadyActive()
    {
        var readingClub = new ReadingClub(
            "Mystery Book Circle",
            "Discussions about mystery novels",
            "Mystery");

        var exception = Assert.Throws<ConflictException>(() =>
            readingClub.Reactivate());

        Assert.Equal("Reading club is already active", exception.Message);
    }

    [Fact]
    public void Reactivate_ShouldThrowConflictException_WhenReadingClubIsArchived()
    {
        var readingClub = new ReadingClub(
            "Fantasy Book Guild",
            "Discussions about fantasy literature",
            "Fantasy");

        readingClub.Archive();

        var exception = Assert.Throws<ConflictException>(() =>
            readingClub.Reactivate());

        Assert.Equal("Archived reading club cannot be reactivated", exception.Message);
    }
}

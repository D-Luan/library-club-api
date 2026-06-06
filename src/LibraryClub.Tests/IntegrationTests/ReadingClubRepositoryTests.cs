using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;
using LibraryClub.Tests.Fixtures;

namespace LibraryClub.Tests.IntegrationTests;

[Trait("Category", "Integration")]
[Collection(IntegrationTestCollection.Name)]
public class ReadingClubRepositoryTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly ReadingClubRepository _repository = fixture.ReadingClubRepository;

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_ShouldInsertReadingClub_WhenReadingClubIsValid()
    {
        var readingClub = new ReadingClub(
            "Classic Books",
            "Monthly classics discussion",
            "Classics");

        await _repository.AddAsync(readingClub);

        var savedReadingClub = await _repository.GetByIdAsync(readingClub.Id);

        Assert.NotNull(savedReadingClub);
        Assert.Equal(readingClub.Id, savedReadingClub.Id);
        Assert.Equal("Classic Books", savedReadingClub.Name);
        Assert.Equal("Monthly classics discussion", savedReadingClub.Description);
        Assert.Equal("Classics", savedReadingClub.Genre);
        Assert.Equal(ReadingClubStatus.Active, savedReadingClub.Status);
        Assert.InRange(
            savedReadingClub.CreatedAt,
            readingClub.CreatedAt.AddSeconds(-1),
            readingClub.CreatedAt.AddSeconds(1));
    }

    [Fact]
    public async Task AddAsync_ShouldInsertReadingClub_WhenDescriptionIsNull()
    {
        var readingClub = new ReadingClub(
            "Sci-Fi Club",
            null,
            "Science Fiction");

        await _repository.AddAsync(readingClub);

        var savedReadingClub = await _repository.GetByIdAsync(readingClub.Id);

        Assert.NotNull(savedReadingClub);
        Assert.Equal(readingClub.Id, savedReadingClub.Id);
        Assert.Equal("Sci-Fi Club", savedReadingClub.Name);
        Assert.Null(savedReadingClub.Description);
        Assert.Equal("Science Fiction", savedReadingClub.Genre);
        Assert.Equal(ReadingClubStatus.Active, savedReadingClub.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReadingClubDoesNotExist()
    {
        var readingClub = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(readingClub);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReadingClubStatusToInactive_WhenReadingClubExists()
    {
        var readingClub = new ReadingClub(
            "Fantasy Club",
            "Fantasy books",
            "Fantasy");

        await _repository.AddAsync(readingClub);

        readingClub.Inactivate();

        await _repository.UpdateAsync(readingClub);

        var updatedReadingClub = await _repository.GetByIdAsync(readingClub.Id);

        Assert.NotNull(updatedReadingClub);
        Assert.Equal(ReadingClubStatus.Inactive, updatedReadingClub.Status);
        Assert.Equal("Fantasy Club", updatedReadingClub.Name);
        Assert.Equal("Fantasy books", updatedReadingClub.Description);
        Assert.Equal("Fantasy", updatedReadingClub.Genre);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReadingClubStatusToArchived_WhenReadingClubExists()
    {
        var readingClub = new ReadingClub(
            "History Club",
            "History books",
            "History");

        await _repository.AddAsync(readingClub);

        readingClub.Archive();

        await _repository.UpdateAsync(readingClub);

        var updatedReadingClub = await _repository.GetByIdAsync(readingClub.Id);

        Assert.NotNull(updatedReadingClub);
        Assert.Equal(ReadingClubStatus.Archived, updatedReadingClub.Status);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReadingClubData_WhenReadingClubExists()
    {
        var readingClub = new ReadingClub(
            "Old Name",
            "Old description",
            "Old genre");

        await _repository.AddAsync(readingClub);

        readingClub.SetName("New Name");
        readingClub.SetDescription("New description");
        readingClub.SetGenre("New genre");

        await _repository.UpdateAsync(readingClub);

        var updatedReadingClub = await _repository.GetByIdAsync(readingClub.Id);

        Assert.NotNull(updatedReadingClub);
        Assert.Equal("New Name", updatedReadingClub.Name);
        Assert.Equal("New description", updatedReadingClub.Description);
        Assert.Equal("New genre", updatedReadingClub.Genre);
    }
}

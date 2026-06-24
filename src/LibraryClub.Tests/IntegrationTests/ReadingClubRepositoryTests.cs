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

        readingClub.UpdateDetails("New Name", "New description", "New genre");

        await _repository.UpdateAsync(readingClub);

        var updatedReadingClub = await _repository.GetByIdAsync(readingClub.Id);

        Assert.NotNull(updatedReadingClub);
        Assert.Equal("New Name", updatedReadingClub.Name);
        Assert.Equal("New description", updatedReadingClub.Description);
        Assert.Equal("New genre", updatedReadingClub.Genre);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnFirstPage_WhenReadingClubsExist()
    {
        var readingClubs = await AddReadingClubsAsync();

        var result = await _repository.GetPagedAsync(page: 1, pageSize: 2);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(readingClubs[2].Id, result.Items[0].Id);
        Assert.Equal(readingClubs[1].Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnSecondPage_WhenReadingClubsExist()
    {
        var readingClubs = await AddReadingClubsAsync();

        var result = await _repository.GetPagedAsync(page: 2, pageSize: 2);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);

        var readingClub = Assert.Single(result.Items);
        Assert.Equal(readingClubs[0].Id, readingClub.Id);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnEmptyPage_WhenReadingClubsDoNotExist()
    {
        var result = await _repository.GetPagedAsync(page: 1, pageSize: 10);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    private async Task<List<ReadingClub>> AddReadingClubsAsync()
    {
        var readingClubs = new List<ReadingClub>();

        for (var index = 1; index <= 3; index++)
        {
            var readingClub = new ReadingClub(
                $"Reading Club {index}",
                $"Description {index}",
                $"Genre {index}");

            await _repository.AddAsync(readingClub);

            readingClubs.Add(readingClub);

            await Task.Delay(10);
        }

        return readingClubs;
    }
}

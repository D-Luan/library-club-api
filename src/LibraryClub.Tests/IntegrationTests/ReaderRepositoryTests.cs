using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;
using LibraryClub.Tests.Fixtures;

namespace LibraryClub.Tests.IntegrationTests;

[Trait("Category", "Integration")]
[Collection(IntegrationTestCollection.Name)]
public class ReaderRepositoryTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly ReaderRepository _repository = fixture.ReaderRepository;
    public Task InitializeAsync() => fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_ShouldInsertReader_WhenReaderIsValid()
    {
        var reader = new Reader("John Doe", "john.doe@email.com");

        await _repository.AddAsync(reader);

        var savedReader = await _repository.GetByIdAsync(reader.Id);

        Assert.NotNull(savedReader);
        Assert.Equal(reader.Id, savedReader.Id);
        Assert.Equal("John Doe", savedReader.Name);
        Assert.Equal("john.doe@email.com", savedReader.Email);
        Assert.Equal(ReaderStatus.Active, savedReader.Status);
        Assert.InRange(
            savedReader.CreatedAt,
            reader.CreatedAt.AddSeconds(-1),
            reader.CreatedAt.AddSeconds(1));
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnReader_WhenEmailExists()
    {
        var reader = new Reader("Mary Taylor", "mary.taylor@email.com");
        await _repository.AddAsync(reader);

        var savedReader = await _repository.GetByEmailAsync("MARY.TAYLOR@EMAIL.COM");

        Assert.NotNull(savedReader);
        Assert.Equal(reader.Id, savedReader.Id);
        Assert.Equal("mary.taylor@email.com", savedReader.Email);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailExists()
    {
        var reader = new Reader("Robert Smith", "robert.smith@email.com");
        await _repository.AddAsync(reader);

        var exists = await _repository.ExistsByEmailAsync("robert.smith@email.com");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        var exists = await _repository.ExistsByEmailAsync("missing@email.com");

        Assert.False(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReader_WhenReaderExists()
    {
        var reader = new Reader("Ana Lima", "ana.lima@email.com");
        await _repository.AddAsync(reader);

        reader.SetName("Ana Maria Lima");
        reader.Inactivate();

        await _repository.UpdateAsync(reader);

        var updatedReader = await _repository.GetByIdAsync(reader.Id);

        Assert.NotNull(updatedReader);
        Assert.Equal("Ana Maria Lima", updatedReader.Name);
        Assert.Equal(ReaderStatus.Inactive, updatedReader.Status);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnFirstPage_WhenReadersExist()
    {
        var readers = await AddReadersAsync();

        var result = await _repository.GetPagedAsync(page: 1, pageSize: 2);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(readers[2].Id, result.Items[0].Id);
        Assert.Equal(readers[1].Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnSecondPage_WhenReadersExist()
    {
        var readers = await AddReadersAsync();

        var result = await _repository.GetPagedAsync(page: 2, pageSize: 2);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);

        var reader = Assert.Single(result.Items);
        Assert.Equal(readers[0].Id, reader.Id);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnEmptyPage_WhenReadersDoNotExist()
    {
        var result = await _repository.GetPagedAsync(page: 1, pageSize: 10);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    private async Task<List<Reader>> AddReadersAsync()
    {
        var readers = new List<Reader>();

        for (var index = 1; index <= 3; index++)
        {
            var reader = new Reader(
                $"Reader {index}",
                $"reader.{index}.{Guid.NewGuid():N}@email.com");

            await _repository.AddAsync(reader);

            readers.Add(reader);

            await Task.Delay(10);
        }

        return readers;
    }
}

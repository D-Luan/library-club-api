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
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("John Doe", "john.doe@email.com");

        await _repository.AddAsync(reader);

        var savedReader = await _repository.GetByIdAsync(reader.Id, cancellationToken);

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
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Mary Taylor", "mary.taylor@email.com");

        await _repository.AddAsync(reader, cancellationToken);

        var savedReader = await _repository.GetByEmailAsync(
            "MARY.TAYLOR@EMAIL.COM",
            cancellationToken);

        Assert.NotNull(savedReader);
        Assert.Equal(reader.Id, savedReader.Id);
        Assert.Equal("mary.taylor@email.com", savedReader.Email);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailExists()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Robert Smith", "robert.smith@email.com");

        await _repository.AddAsync(reader, cancellationToken);

        var exists = await _repository.ExistsByEmailAsync(
            "robert.smith@email.com",
            cancellationToken);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        var cancellationToken = CancellationToken.None;

        var exists = await _repository.ExistsByEmailAsync(
            "missing@email.com",
            cancellationToken);

        Assert.False(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReader_WhenReaderExists()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Ana Lima", "ana.lima@email.com");

        await _repository.AddAsync(reader, cancellationToken);

        reader.ChangeName("Ana Maria Lima");
        reader.Inactivate();

        await _repository.UpdateAsync(reader, cancellationToken);

        var updatedReader = await _repository.GetByIdAsync(reader.Id, cancellationToken);

        Assert.NotNull(updatedReader);
        Assert.Equal("Ana Maria Lima", updatedReader.Name);
        Assert.Equal(ReaderStatus.Inactive, updatedReader.Status);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnFirstPage_WhenReadersExist()
    {
        var cancellationToken = CancellationToken.None;

        var readers = await AddReadersAsync(cancellationToken);

        var result = await _repository.GetPagedAsync(
            page: 1,
            pageSize: 2,
            cancellationToken);

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
        var cancellationToken = CancellationToken.None;

        var readers = await AddReadersAsync(cancellationToken);

        var result = await _repository.GetPagedAsync(
            page: 2,
            pageSize: 2,
            cancellationToken);

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
        var cancellationToken = CancellationToken.None;

        var result = await _repository.GetPagedAsync(
            page: 1,
            pageSize: 10,
            cancellationToken);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    private async Task<List<Reader>> AddReadersAsync(CancellationToken cancellationToken = default)
    {
        var readers = new List<Reader>
        {
            Reader.Restore(
                Guid.NewGuid(),
                "Reader 1",
                $"reader.1.{Guid.NewGuid():N}@email.com",
                ReaderStatus.Active,
                DateTime.UtcNow.AddMinutes(-3)),

            Reader.Restore(
                Guid.NewGuid(),
                "Reader 2",
                $"reader.2.{Guid.NewGuid():N}@email.com",
                ReaderStatus.Active,
                DateTime.UtcNow.AddMinutes(-2)),

            Reader.Restore(
                Guid.NewGuid(),
                "Reader 3",
                $"reader.3.{Guid.NewGuid():N}@email.com",
                ReaderStatus.Active,
                DateTime.UtcNow.AddMinutes(-1))
        };

        foreach (var reader in readers)
        {
            await _repository.AddAsync(reader, cancellationToken);
        }

        return readers;
    }
}
using LibraryClub.Api.Data;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;
using LibraryClub.Tests.Fixtures;

namespace LibraryClub.Tests.IntegrationTests;

public sealed class ReaderRepositoryTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    private readonly ReaderRepository _repository = new(new SqlConnectionFactory(fixture.ConnectionString));

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
}

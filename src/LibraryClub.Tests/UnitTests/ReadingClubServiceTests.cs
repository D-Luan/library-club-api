using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;
using LibraryClub.Api.Services;
using LibraryClub.Api.Common;
using NSubstitute;
using Microsoft.Extensions.Logging.Abstractions;

namespace LibraryClub.Tests.UnitTests;

[Trait("Category", "Unit")]
public class ReadingClubServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateReadingClub()
    {
        var repository = Substitute.For<IReadingClubRepository>();
        var service = new ReadingClubService(repository, NullLogger<ReadingClubService>.Instance);

        var readingClub = await service.CreateAsync("Fantasy Club", "Fantasy books", "Fantasy");

        Assert.NotEqual(Guid.Empty, readingClub.Id);
        Assert.Equal("Fantasy Club", readingClub.Name);
        Assert.Equal("Fantasy books", readingClub.Description);
        Assert.Equal("Fantasy", readingClub.Genre);
        Assert.Equal(ReadingClubStatus.Active, readingClub.Status);

        await repository.Received(1).AddAsync(Arg.Is<ReadingClub>(r =>
            r.Id == readingClub.Id &&
            r.Name == "Fantasy Club" &&
            r.Description == "Fantasy books" &&
            r.Genre == "Fantasy" &&
            r.Status == ReadingClubStatus.Active));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReadingClub_WhenReadingClubExists()
    {
        var repository = Substitute.For<IReadingClubRepository>();
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        repository.GetByIdAsync(readingClub.Id).Returns(readingClub);

        var service = new ReadingClubService(repository, NullLogger<ReadingClubService>.Instance);

        var result = await service.GetByIdAsync(readingClub.Id);

        Assert.NotNull(result);
        Assert.Equal(readingClub.Id, result.Id);

        await repository.Received(1).GetByIdAsync(readingClub.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReadingClubDoesNotExist()
    {
        var repository = Substitute.For<IReadingClubRepository>();
        var id = Guid.NewGuid();

        repository.GetByIdAsync(id).Returns((ReadingClub?)null);

        var service = new ReadingClubService(repository, NullLogger<ReadingClubService>.Instance);

        var result = await service.GetByIdAsync(id);

        Assert.Null(result);

        await repository.Received(1).GetByIdAsync(id);
    }

    [Fact]
    public async Task InactivateAsync_ShouldInactivateReadingClub_WhenReadingClubExists()
    {
        var repository = Substitute.For<IReadingClubRepository>();
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        repository.GetByIdAsync(readingClub.Id).Returns(readingClub);

        var service = new ReadingClubService(repository, NullLogger<ReadingClubService>.Instance);

        await service.InactivateAsync(readingClub.Id);

        Assert.Equal(ReadingClubStatus.Inactive, readingClub.Status);

        await repository.Received(1).UpdateAsync(Arg.Is<ReadingClub>(r =>
            r.Id == readingClub.Id &&
            r.Status == ReadingClubStatus.Inactive));
    }

    [Fact]
    public async Task InactivateAsync_ShouldThrowException_WhenReadingClubDoesNotExist()
    {
        var repository = Substitute.For<IReadingClubRepository>();
        var id = Guid.NewGuid();

        repository.GetByIdAsync(id).Returns((ReadingClub?)null);

        var service = new ReadingClubService(repository, NullLogger<ReadingClubService>.Instance);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.InactivateAsync(id));

        Assert.Equal("Reading club not found", exception.Message);

        await repository.DidNotReceive().UpdateAsync(Arg.Any<ReadingClub>());
    }

    [Fact]
    public async Task ArchiveAsync_ShouldArchiveReadingClub_WhenReadingClubExists()
    {
        var repository = Substitute.For<IReadingClubRepository>();
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        repository.GetByIdAsync(readingClub.Id).Returns(readingClub);

        var service = new ReadingClubService(repository, NullLogger<ReadingClubService>.Instance);

        await service.ArchiveAsync(readingClub.Id);

        Assert.Equal(ReadingClubStatus.Archived, readingClub.Status);

        await repository.Received(1).UpdateAsync(Arg.Is<ReadingClub>(r =>
            r.Id == readingClub.Id &&
            r.Status == ReadingClubStatus.Archived));
    }

    [Fact]
    public async Task ArchiveAsync_ShouldThrowException_WhenReadingClubDoesNotExist()
    {
        var repository = Substitute.For<IReadingClubRepository>();
        var id = Guid.NewGuid();

        repository.GetByIdAsync(id).Returns((ReadingClub?)null);

        var service = new ReadingClubService(repository, NullLogger<ReadingClubService>.Instance);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.ArchiveAsync(id));

        Assert.Equal("Reading club not found", exception.Message);

        await repository.DidNotReceive().UpdateAsync(Arg.Any<ReadingClub>());
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedReadingClubs()
    {
        var repository = Substitute.For<IReadingClubRepository>();

        var readingClubs = new List<ReadingClub>
        {
            new("Fantasy Club", "Fantasy books", "Fantasy"),
            new("History Club", "History books", "History")
        };

        var pagedResult = new PagedResult<ReadingClub>(
            readingClubs,
            Page: 1,
            PageSize: 2,
            TotalCount: 3);

        repository.GetPagedAsync(1, 2).Returns(pagedResult);

        var service = new ReadingClubService(repository, NullLogger<ReadingClubService>.Instance);

        var result = await service.GetPagedAsync(1, 2);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        await repository.Received(1).GetPagedAsync(1, 2);
    }
}

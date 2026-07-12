using LibraryClub.Api.Common;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;
using LibraryClub.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LibraryClub.Tests.UnitTests;

[Trait("Category", "Unit")]
public class ReadingClubServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateReadingClub_WhenDataIsValid()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var service = context.CreateService();

        var readingClub = await service.CreateAsync(
            "Epic Worlds Club",
            "Fantasy literature discussions",
            "Fantasy",
            cancellationToken);

        Assert.NotEqual(Guid.Empty, readingClub.Id);
        Assert.Equal("Epic Worlds Club", readingClub.Name);
        Assert.Equal("Fantasy literature discussions", readingClub.Description);
        Assert.Equal("Fantasy", readingClub.Genre);
        Assert.Equal(ReadingClubStatus.Active, readingClub.Status);

        await context.Repository.Received(1).AddAsync(
            Arg.Is<ReadingClub>(club =>
                club.Id == readingClub.Id &&
                club.Name == "Epic Worlds Club" &&
                club.Description == "Fantasy literature discussions" &&
                club.Genre == "Fantasy" &&
                club.Status == ReadingClubStatus.Active),
            cancellationToken);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReadingClub_WhenReadingClubExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub("Mythic Pages Circle", null, "Fantasy");

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var result = await service.GetByIdAsync(readingClub.Id, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(readingClub.Id, result.Id);

        await context.Repository.Received(1)
            .GetByIdAsync(readingClub.Id, cancellationToken);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReadingClubDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var id = Guid.NewGuid();

        context.Repository
            .GetByIdAsync(id, cancellationToken)
            .Returns((ReadingClub?)null);

        var service = context.CreateService();

        var result = await service.GetByIdAsync(id, cancellationToken);

        Assert.Null(result);

        await context.Repository.Received(1)
            .GetByIdAsync(id, cancellationToken);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedReadingClubs()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClubs = new List<ReadingClub>
        {
            new("Adventure Readers", "Adventure novels", "Adventure"),
            new("History Club", "History books", "History")
        };

        var pagedResult = new PagedResult<ReadingClub>(
            readingClubs,
            Page: 1,
            PageSize: 2,
            TotalCount: 3);

        context.Repository
            .GetPagedAsync(1, 2, cancellationToken)
            .Returns(pagedResult);

        var service = context.CreateService();

        var result = await service.GetPagedAsync(
            page: 1,
            pageSize: 2,
            cancellationToken);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        await context.Repository.Received(1)
            .GetPagedAsync(1, 2, cancellationToken);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReadingClub_WhenReadingClubExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub(
            "Coastal Classics",
            "Monthly discussions on classic literature",
            "Classics");

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        await service.UpdateAsync(
            readingClub.Id,
            "Mystery Book Circle",
            "Discussions about mystery novels",
            "Mystery",
            cancellationToken);

        Assert.Equal("Mystery Book Circle", readingClub.Name);
        Assert.Equal("Discussions about mystery novels", readingClub.Description);
        Assert.Equal("Mystery", readingClub.Genre);
        Assert.Equal(ReadingClubStatus.Active, readingClub.Status);

        await context.Repository.Received(1)
            .GetByIdAsync(readingClub.Id, cancellationToken);

        await context.Repository.Received(1).UpdateAsync(
            Arg.Is<ReadingClub>(club =>
                club.Id == readingClub.Id &&
                club.Name == "Mystery Book Circle" &&
                club.Description == "Discussions about mystery novels" &&
                club.Genre == "Mystery" &&
                club.Status == ReadingClubStatus.Active),
            cancellationToken);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateInactiveReadingClub_WhenReadingClubExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub(
            "Science Fiction Society",
            "Exploring speculative fiction",
            "Science Fiction");

        readingClub.Inactivate();

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        await service.UpdateAsync(
            readingClub.Id,
            "Historical Fiction Forum",
            null,
            "Historical Fiction",
            cancellationToken);

        Assert.Equal("Historical Fiction Forum", readingClub.Name);
        Assert.Null(readingClub.Description);
        Assert.Equal("Historical Fiction", readingClub.Genre);
        Assert.Equal(ReadingClubStatus.Inactive, readingClub.Status);

        await context.Repository.Received(1)
            .UpdateAsync(readingClub, cancellationToken);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFound_WhenReadingClubDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClubId = Guid.NewGuid();

        context.Repository
            .GetByIdAsync(readingClubId, cancellationToken)
            .Returns((ReadingClub?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(
                readingClubId,
                "Poetry Reading Circle",
                "Weekly readings of contemporary poetry",
                "Poetry",
                cancellationToken));

        Assert.Equal("Reading club not found", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowConflict_WhenReadingClubIsArchived()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub(
            "Fantasy Book Guild",
            "Discussions about fantasy literature",
            "Fantasy");

        readingClub.Archive();

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateAsync(
                readingClub.Id,
                "Literary Fiction Circle",
                "Discussions about contemporary literary fiction",
                "Literary Fiction",
                cancellationToken));

        Assert.Equal("Archived reading club cannot be updated", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InactivateAsync_ShouldInactivateReadingClub_WhenReadingClubExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub("Epic Worlds Club", null, "Fantasy");

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        await service.InactivateAsync(readingClub.Id, cancellationToken);

        Assert.Equal(ReadingClubStatus.Inactive, readingClub.Status);

        await context.Repository.Received(1).UpdateAsync(
            Arg.Is<ReadingClub>(club =>
                club.Id == readingClub.Id &&
                club.Status == ReadingClubStatus.Inactive),
            cancellationToken);
    }

    [Fact]
    public async Task InactivateAsync_ShouldThrowNotFound_WhenReadingClubDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var id = Guid.NewGuid();

        context.Repository
            .GetByIdAsync(id, cancellationToken)
            .Returns((ReadingClub?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.InactivateAsync(id, cancellationToken));

        Assert.Equal("Reading club not found", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InactivateAsync_ShouldThrowConflict_WhenReadingClubIsAlreadyInactive()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub("Ancient History Club", null, "History");
        readingClub.Inactivate();

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.InactivateAsync(readingClub.Id, cancellationToken));

        Assert.Equal("Reading club is already inactive", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InactivateAsync_ShouldThrowConflict_WhenReadingClubIsArchived()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub("Poetry Reading Circle", null, "Poetry");
        readingClub.Archive();

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.InactivateAsync(readingClub.Id, cancellationToken));

        Assert.Equal("Archived reading club cannot be inactivated", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveAsync_ShouldArchiveReadingClub_WhenReadingClubExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub("Noir Book Society", null, "Noir");

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        await service.ArchiveAsync(readingClub.Id, cancellationToken);

        Assert.Equal(ReadingClubStatus.Archived, readingClub.Status);

        await context.Repository.Received(1).UpdateAsync(
            Arg.Is<ReadingClub>(club =>
                club.Id == readingClub.Id &&
                club.Status == ReadingClubStatus.Archived),
            cancellationToken);
    }

    [Fact]
    public async Task ArchiveAsync_ShouldThrowNotFound_WhenReadingClubDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var id = Guid.NewGuid();

        context.Repository
            .GetByIdAsync(id, cancellationToken)
            .Returns((ReadingClub?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ArchiveAsync(id, cancellationToken));

        Assert.Equal("Reading club not found", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveAsync_ShouldThrowConflict_WhenReadingClubIsAlreadyArchived()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub("Adventure Readers", null, "Adventure");
        readingClub.Archive();

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.ArchiveAsync(readingClub.Id, cancellationToken));

        Assert.Equal("Reading club is already archived", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateReadingClub_WhenReadingClubIsInactive()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub(
            "Historical Fiction Forum",
            "Discussions about historical novels",
            "Historical Fiction");

        readingClub.Inactivate();

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        await service.ReactivateAsync(readingClub.Id, cancellationToken);

        Assert.Equal(ReadingClubStatus.Active, readingClub.Status);

        await context.Repository.Received(1)
            .GetByIdAsync(readingClub.Id, cancellationToken);

        await context.Repository.Received(1).UpdateAsync(
            Arg.Is<ReadingClub>(club =>
                club.Id == readingClub.Id &&
                club.Status == ReadingClubStatus.Active),
            cancellationToken);
    }

    [Fact]
    public async Task ReactivateAsync_ShouldThrowNotFound_WhenReadingClubDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClubId = Guid.NewGuid();

        context.Repository
            .GetByIdAsync(readingClubId, cancellationToken)
            .Returns((ReadingClub?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ReactivateAsync(readingClubId, cancellationToken));

        Assert.Equal("Reading club not found", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateAsync_ShouldThrowConflict_WhenReadingClubIsAlreadyActive()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub(
            "Mystery Book Circle",
            "Discussions about mystery novels",
            "Mystery");

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.ReactivateAsync(readingClub.Id, cancellationToken));

        Assert.Equal("Reading club is already active", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateAsync_ShouldThrowConflict_WhenReadingClubIsArchived()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub(
            "Fantasy Book Guild",
            "Discussions about fantasy literature",
            "Fantasy");

        readingClub.Archive();

        context.Repository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.ReactivateAsync(readingClub.Id, cancellationToken));

        Assert.Equal("Archived reading club cannot be reactivated", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<ReadingClub>(),
            Arg.Any<CancellationToken>());
    }

    private sealed class TestContext
    {
        public IReadingClubRepository Repository { get; } =
            Substitute.For<IReadingClubRepository>();

        public ReadingClubService CreateService()
        {
            return new ReadingClubService(
                Repository,
                NullLogger<ReadingClubService>.Instance);
        }
    }
}

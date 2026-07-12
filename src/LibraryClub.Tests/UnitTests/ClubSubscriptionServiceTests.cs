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
public class ClubSubscriptionServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateSubscription_WhenDataIsValid()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Clara Bennett", "clara.bennett@email.com");
        var readingClub = new ReadingClub("Epic Worlds Club", null, "Fantasy");

        context.ReaderRepository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        context.ReadingClubRepository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        context.SubscriptionRepository
            .ExistsActiveAsync(reader.Id, readingClub.Id, cancellationToken)
            .Returns(false);

        var service = context.CreateService();

        var subscription = await service.CreateAsync(
            reader.Id,
            readingClub.Id,
            cancellationToken);

        Assert.Equal(reader.Id, subscription.ReaderId);
        Assert.Equal(readingClub.Id, subscription.ReadingClubId);
        Assert.Equal(ClubSubscriptionStatus.Active, subscription.Status);

        await context.SubscriptionRepository.Received(1).AddAsync(
            Arg.Is<ClubSubscription>(s =>
                s.ReaderId == reader.Id &&
                s.ReadingClubId == readingClub.Id &&
                s.Status == ClubSubscriptionStatus.Active),
            cancellationToken);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenReaderDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readerId = Guid.NewGuid();
        var readingClubId = Guid.NewGuid();

        context.ReaderRepository
            .GetByIdAsync(readerId, cancellationToken)
            .Returns((Reader?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(readerId, readingClubId, cancellationToken));

        Assert.Equal("Reader not found", exception.Message);

        await context.SubscriptionRepository.DidNotReceive().AddAsync(
            Arg.Any<ClubSubscription>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenReadingClubDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Thiago Smith", "smith@email.com");
        var readingClubId = Guid.NewGuid();

        context.ReaderRepository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        context.ReadingClubRepository
            .GetByIdAsync(readingClubId, cancellationToken)
            .Returns((ReadingClub?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(reader.Id, readingClubId, cancellationToken));

        Assert.Equal("Reading club not found", exception.Message);

        await context.SubscriptionRepository.DidNotReceive().AddAsync(
            Arg.Any<ClubSubscription>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenReaderIsInactive()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Mary Taylor", "mary.taylor@email.com");
        reader.Inactivate();

        var readingClub = new ReadingClub("Drama Club", null, "Drama");

        context.ReaderRepository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        context.ReadingClubRepository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(reader.Id, readingClub.Id, cancellationToken));

        Assert.Equal("Reader is inactive", exception.Message);

        await context.SubscriptionRepository.DidNotReceive().AddAsync(
            Arg.Any<ClubSubscription>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenReadingClubIsInactive()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Paul Marston", "paul@email.com");

        var readingClub = new ReadingClub("Science Club", null, "Sci-fi");
        readingClub.Inactivate();

        context.ReaderRepository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        context.ReadingClubRepository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(reader.Id, readingClub.Id, cancellationToken));

        Assert.Equal("Reading club is inactive", exception.Message);

        await context.SubscriptionRepository.DidNotReceive().AddAsync(
            Arg.Any<ClubSubscription>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenReadingClubIsArchived()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Ana Morgan", "ana.morgan@email.com");

        var readingClub = new ReadingClub("History Club", null, "History");
        readingClub.Archive();

        context.ReaderRepository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        context.ReadingClubRepository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(reader.Id, readingClub.Id, cancellationToken));

        Assert.Equal("Reading club is archived", exception.Message);

        await context.SubscriptionRepository.DidNotReceive().AddAsync(
            Arg.Any<ClubSubscription>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenActiveSubscriptionAlreadyExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Enzo Rodriguez", "rodriguez@email.com");
        var readingClub = new ReadingClub("Romance Club", null, "Romance");

        context.ReaderRepository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        context.ReadingClubRepository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        context.SubscriptionRepository
            .ExistsActiveAsync(reader.Id, readingClub.Id, cancellationToken)
            .Returns(true);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(reader.Id, readingClub.Id, cancellationToken));

        Assert.Equal(
            "Reader already has an active subscription for this reading club",
            exception.Message);

        await context.SubscriptionRepository.DidNotReceive().AddAsync(
            Arg.Any<ClubSubscription>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSubscription_WhenSubscriptionExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());

        context.SubscriptionRepository
            .GetByIdAsync(subscription.Id, cancellationToken)
            .Returns(subscription);

        var service = context.CreateService();

        var result = await service.GetByIdAsync(subscription.Id, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(subscription.Id, result.Id);
    }

    [Fact]
    public async Task GetByReaderAsync_ShouldReturnPagedSubscriptions_WhenReaderExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Taylor Smith", "taylor@email.com");

        var expected = new PagedResult<ClubSubscription>(
            new List<ClubSubscription>
            {
                new(reader.Id, Guid.NewGuid()),
                new(reader.Id, Guid.NewGuid())
            },
            Page: 1,
            PageSize: 2,
            TotalCount: 3);

        context.ReaderRepository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        context.SubscriptionRepository
            .GetByReaderAsync(reader.Id, 1, 2, cancellationToken)
            .Returns(expected);

        var service = context.CreateService();

        var result = await service.GetByReaderAsync(
            reader.Id,
            page: 1,
            pageSize: 2,
            cancellationToken);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        await context.ReaderRepository.Received(1)
            .GetByIdAsync(reader.Id, cancellationToken);

        await context.SubscriptionRepository.Received(1)
            .GetByReaderAsync(reader.Id, 1, 2, cancellationToken);
    }

    [Fact]
    public async Task GetByReaderAsync_ShouldThrowNotFound_WhenReaderDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readerId = Guid.NewGuid();

        context.ReaderRepository
            .GetByIdAsync(readerId, cancellationToken)
            .Returns((Reader?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByReaderAsync(
                readerId,
                page: 1,
                pageSize: 10,
                cancellationToken));

        Assert.Equal("Reader not found", exception.Message);

        await context.SubscriptionRepository.DidNotReceive()
            .GetByReaderAsync(
                Arg.Any<Guid>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByReadingClubAsync_ShouldReturnPagedSubscriptions_WhenReadingClubExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub("Mythic Pages Circle", null, "Fantasy");

        var expected = new PagedResult<ClubSubscription>(
            new List<ClubSubscription>
            {
                new(Guid.NewGuid(), readingClub.Id),
                new(Guid.NewGuid(), readingClub.Id)
            },
            Page: 1,
            PageSize: 2,
            TotalCount: 3);

        context.ReadingClubRepository
            .GetByIdAsync(readingClub.Id, cancellationToken)
            .Returns(readingClub);

        context.SubscriptionRepository
            .GetByReadingClubAsync(readingClub.Id, 1, 2, cancellationToken)
            .Returns(expected);

        var service = context.CreateService();

        var result = await service.GetByReadingClubAsync(
            readingClub.Id,
            page: 1,
            pageSize: 2,
            cancellationToken);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        await context.ReadingClubRepository.Received(1)
            .GetByIdAsync(readingClub.Id, cancellationToken);

        await context.SubscriptionRepository.Received(1)
            .GetByReadingClubAsync(readingClub.Id, 1, 2, cancellationToken);
    }

    [Fact]
    public async Task GetByReadingClubAsync_ShouldThrowNotFound_WhenReadingClubDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readingClubId = Guid.NewGuid();

        context.ReadingClubRepository
            .GetByIdAsync(readingClubId, cancellationToken)
            .Returns((ReadingClub?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByReadingClubAsync(
                readingClubId,
                page: 1,
                pageSize: 10,
                cancellationToken));

        Assert.Equal("Reading club not found", exception.Message);

        await context.SubscriptionRepository.DidNotReceive()
            .GetByReadingClubAsync(
                Arg.Any<Guid>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelSubscription_WhenSubscriptionExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());

        context.SubscriptionRepository
            .GetByIdAsync(subscription.Id, cancellationToken)
            .Returns(subscription);

        var service = context.CreateService();

        await service.CancelAsync(subscription.Id, cancellationToken);

        Assert.Equal(ClubSubscriptionStatus.Canceled, subscription.Status);
        Assert.NotNull(subscription.CanceledAt);

        await context.SubscriptionRepository.Received(1)
            .UpdateAsync(subscription, cancellationToken);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowNotFound_WhenSubscriptionDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var id = Guid.NewGuid();

        context.SubscriptionRepository
            .GetByIdAsync(id, cancellationToken)
            .Returns((ClubSubscription?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CancelAsync(id, cancellationToken));

        Assert.Equal("Club subscription not found", exception.Message);

        await context.SubscriptionRepository.DidNotReceive()
            .UpdateAsync(
                Arg.Any<ClubSubscription>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowConflict_WhenSubscriptionIsAlreadyCanceled()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());
        subscription.Cancel();

        context.SubscriptionRepository
            .GetByIdAsync(subscription.Id, cancellationToken)
            .Returns(subscription);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CancelAsync(subscription.Id, cancellationToken));

        Assert.Equal("Club subscription is already canceled", exception.Message);

        await context.SubscriptionRepository.DidNotReceive()
            .UpdateAsync(
                Arg.Any<ClubSubscription>(),
                Arg.Any<CancellationToken>());
    }

    private sealed class TestContext
    {
        public IClubSubscriptionRepository SubscriptionRepository { get; } =
            Substitute.For<IClubSubscriptionRepository>();

        public IReaderRepository ReaderRepository { get; } =
            Substitute.For<IReaderRepository>();

        public IReadingClubRepository ReadingClubRepository { get; } =
            Substitute.For<IReadingClubRepository>();

        public ClubSubscriptionService CreateService()
        {
            return new ClubSubscriptionService(
                SubscriptionRepository,
                ReaderRepository,
                ReadingClubRepository,
                NullLogger<ClubSubscriptionService>.Instance);
        }
    }
}

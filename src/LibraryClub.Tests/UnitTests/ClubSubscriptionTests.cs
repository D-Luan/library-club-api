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
public class ClubSubscriptionTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveSubscription_WhenDataIsValid()
    {
        var readerId = Guid.NewGuid();
        var readingClubId = Guid.NewGuid();

        var subscription = new ClubSubscription(readerId, readingClubId);

        Assert.NotEqual(Guid.Empty, subscription.Id);
        Assert.Equal(readerId, subscription.ReaderId);
        Assert.Equal(readingClubId, subscription.ReadingClubId);
        Assert.Equal(ClubSubscriptionStatus.Active, subscription.Status);
        Assert.NotEqual(default, subscription.CreatedAt);
        Assert.Null(subscription.CanceledAt);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenReaderIdIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new ClubSubscription(Guid.Empty, Guid.NewGuid()));

        Assert.Equal("Reader id cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenReadingClubIdIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new ClubSubscription(Guid.NewGuid(), Guid.Empty));

        Assert.Equal("Reading club id cannot be empty", exception.Message);
    }

    [Fact]
    public void Restore_ShouldCreateActiveSubscription_WhenDataIsValid()
    {
        var id = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var readingClubId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);

        var subscription = ClubSubscription.Restore(
            id,
            readerId,
            readingClubId,
            ClubSubscriptionStatus.Active,
            createdAt,
            canceledAt: null);

        Assert.Equal(id, subscription.Id);
        Assert.Equal(readerId, subscription.ReaderId);
        Assert.Equal(readingClubId, subscription.ReadingClubId);
        Assert.Equal(ClubSubscriptionStatus.Active, subscription.Status);
        Assert.Equal(createdAt, subscription.CreatedAt);
        Assert.Null(subscription.CanceledAt);
    }

    [Fact]
    public void Restore_ShouldCreateCanceledSubscription_WhenDataIsValid()
    {
        var id = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var readingClubId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-2);
        var canceledAt = createdAt.AddDays(1);

        var subscription = ClubSubscription.Restore(
            id,
            readerId,
            readingClubId,
            ClubSubscriptionStatus.Canceled,
            createdAt,
            canceledAt);

        Assert.Equal(id, subscription.Id);
        Assert.Equal(readerId, subscription.ReaderId);
        Assert.Equal(readingClubId, subscription.ReadingClubId);
        Assert.Equal(ClubSubscriptionStatus.Canceled, subscription.Status);
        Assert.Equal(createdAt, subscription.CreatedAt);
        Assert.Equal(canceledAt, subscription.CanceledAt);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCanceledAndRecordCancellationDate_WhenSubscriptionIsActive()
    {
        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());
        var beforeCancel = DateTime.UtcNow;

        subscription.Cancel();

        Assert.Equal(ClubSubscriptionStatus.Canceled, subscription.Status);
        Assert.NotNull(subscription.CanceledAt);
        Assert.True(subscription.CanceledAt >= beforeCancel);
        Assert.True(subscription.CanceledAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenSubscriptionIsAlreadyCanceled()
    {
        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());
        subscription.Cancel();

        var exception = Assert.Throws<ConflictException>(() => subscription.Cancel());

        Assert.Equal("Club subscription is already canceled", exception.Message);
    }

    [Fact]
    public async Task GetByReaderAsync_ShouldReturnPagedSubscriptions_WhenReaderExists()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var reader = new Reader("Taylor Smith", "taylor@email.com");

        var expected = new PagedResult<ClubSubscription>(new List<ClubSubscription>
            {
              new(reader.Id, Guid.NewGuid()),
              new(reader.Id, Guid.NewGuid())
            },
            Page: 1,
            PageSize: 2,
            TotalCount: 3);

        readerRepository.GetByIdAsync(reader.Id).Returns(reader);
        subscriptionRepository.GetByReaderAsync(reader.Id, 1, 2).Returns(expected);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository,
            NullLogger<ClubSubscriptionService>.Instance);

        var result = await service.GetByReaderAsync(reader.Id, 1, 2);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        await readerRepository.Received(1).GetByIdAsync(reader.Id);
        await subscriptionRepository.Received(1).GetByReaderAsync(reader.Id, 1, 2);
    }

    [Fact]
    public async Task GetByReaderAsync_ShouldThrowNotFound_WhenReaderDoesNotExist()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();
        var readerId = Guid.NewGuid();

        readerRepository.GetByIdAsync(readerId).Returns((Reader?)null);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository,
            NullLogger<ClubSubscriptionService>.Instance);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByReaderAsync(readerId, 1, 10));

        Assert.Equal("Reader not found", exception.Message);

        await subscriptionRepository.DidNotReceive()
            .GetByReaderAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task GetByReadingClubAsync_ShouldReturnPagedSubscriptions_WhenReadingClubExists()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        var expected = new PagedResult<ClubSubscription>(new List<ClubSubscription>
            {
              new(Guid.NewGuid(), readingClub.Id),
              new(Guid.NewGuid(), readingClub.Id)
            },
            Page: 1,
            PageSize: 2,
            TotalCount: 3);

        readingClubRepository.GetByIdAsync(readingClub.Id).Returns(readingClub);
        subscriptionRepository.GetByReadingClubAsync(readingClub.Id, 1, 2).Returns(expected);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository,
            NullLogger<ClubSubscriptionService>.Instance);

        var result = await service.GetByReadingClubAsync(readingClub.Id, 1, 2);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        await readingClubRepository.Received(1).GetByIdAsync(readingClub.Id);
        await subscriptionRepository.Received(1).GetByReadingClubAsync(readingClub.Id, 1, 2);
    }

    [Fact]
    public async Task GetByReadingClubAsync_ShouldThrowNotFound_WhenReadingClubDoesNotExist()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();
        var readingClubId = Guid.NewGuid();

        readingClubRepository.GetByIdAsync(readingClubId).Returns((ReadingClub?)null);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository,
            NullLogger<ClubSubscriptionService>.Instance);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByReadingClubAsync(readingClubId, 1, 10));

        Assert.Equal("Reading club not found", exception.Message);

        await subscriptionRepository.DidNotReceive()
            .GetByReadingClubAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenIdIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.Empty,
                Guid.NewGuid(),
                Guid.NewGuid(),
                ClubSubscriptionStatus.Active,
                DateTime.UtcNow,
                canceledAt: null));

        Assert.Equal("Club subscription id cannot be empty", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenCreatedAtIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ClubSubscriptionStatus.Active,
                default,
                canceledAt: null));

        Assert.Equal("Club subscription creation date cannot be empty", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenStatusIsInvalid()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                (ClubSubscriptionStatus)999,
                DateTime.UtcNow,
                canceledAt: null));

        Assert.Equal("Invalid club subscription status", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenActiveSubscriptionHasCancellationDate()
    {
        var createdAt = DateTime.UtcNow.AddDays(-1);

        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ClubSubscriptionStatus.Active,
                createdAt,
                createdAt.AddHours(1)));

        Assert.Equal("Active club subscription cannot have cancellation date", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenCanceledSubscriptionDoesNotHaveCancellationDate()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ClubSubscriptionStatus.Canceled,
                DateTime.UtcNow,
                canceledAt: null));

        Assert.Equal("Canceled club subscription must have cancellation date", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenCancellationDateIsEmpty()
    {
        var createdAt = DateTime.UtcNow.AddDays(-1);

        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ClubSubscriptionStatus.Canceled,
                createdAt,
                default));

        Assert.Equal("Canceled club subscription must have cancellation date", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenCancellationDateIsEarlierThanCreationDate()
    {
        var createdAt = DateTime.UtcNow;

        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ClubSubscriptionStatus.Canceled,
                createdAt,
                createdAt.AddMinutes(-1)));

        Assert.Equal("Club subscription cancellation date cannot be earlier than creation date",
        exception.Message);
    }
}

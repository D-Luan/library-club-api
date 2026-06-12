using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;
using LibraryClub.Api.Services;
using NSubstitute;

namespace LibraryClub.Tests.UnitTests;

[Trait("Category", "Unit")]
public class ClubSubscriptionServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateSubscription_WhenDataIsValid()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var reader = new Reader("John Doe", "john@email.com");
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        readerRepository.GetByIdAsync(reader.Id).Returns(reader);
        readingClubRepository.GetByIdAsync(readingClub.Id).Returns(readingClub);
        subscriptionRepository.ExistsActiveAsync(reader.Id, readingClub.Id).Returns(false);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        var subscription = await service.CreateAsync(reader.Id, readingClub.Id);

        Assert.Equal(reader.Id, subscription.ReaderId);
        Assert.Equal(readingClub.Id, subscription.ReadingClubId);
        Assert.Equal(ClubSubscriptionStatus.Active, subscription.Status);

        await subscriptionRepository.Received(1).AddAsync(Arg.Is<ClubSubscription>(s =>
            s.ReaderId == reader.Id &&
            s.ReadingClubId == readingClub.Id &&
            s.Status == ClubSubscriptionStatus.Active));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenReaderDoesNotExist()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var readerId = Guid.NewGuid();

        readerRepository.GetByIdAsync(readerId).Returns((Reader?)null);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(readerId, Guid.NewGuid()));

        Assert.Equal("Reader not found", exception.Message);

        await subscriptionRepository.DidNotReceive().AddAsync(Arg.Any<ClubSubscription>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenReadingClubDoesNotExist()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var reader = new Reader("Thiago Smith", "smith@email.com");
        var readingClubId = Guid.NewGuid();

        readerRepository.GetByIdAsync(reader.Id).Returns(reader);
        readingClubRepository.GetByIdAsync(readingClubId).Returns((ReadingClub?)null);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(reader.Id, readingClubId));

        Assert.Equal("Reading club not found", exception.Message);

        await subscriptionRepository.DidNotReceive().AddAsync(Arg.Any<ClubSubscription>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenReaderIsInactive()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var reader = new Reader("Mary Taylor", "mary.taylor@email.com");
        reader.Inactivate();

        var readingClub = new ReadingClub("Drama Club", null, "Drama");

        readerRepository.GetByIdAsync(reader.Id).Returns(reader);
        readingClubRepository.GetByIdAsync(readingClub.Id).Returns(readingClub);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(reader.Id, readingClub.Id));

        Assert.Equal("Reader is inactive", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenReadingClubIsInactive()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var reader = new Reader("Paul Marston", "paul@email.com");
        var readingClub = new ReadingClub("Science Club", null, "Sci-fi");
        readingClub.Inactivate();

        readerRepository.GetByIdAsync(reader.Id).Returns(reader);
        readingClubRepository.GetByIdAsync(readingClub.Id).Returns(readingClub);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(reader.Id, readingClub.Id));

        Assert.Equal("Reading club is inactive", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenActiveSubscriptionAlreadyExists()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var reader = new Reader("Enzo Rodriguez", "rodriguez@email.com");
        var readingClub = new ReadingClub("Romance Club", null, "Romance");

        readerRepository.GetByIdAsync(reader.Id).Returns(reader);
        readingClubRepository.GetByIdAsync(readingClub.Id).Returns(readingClub);
        subscriptionRepository.ExistsActiveAsync(reader.Id, readingClub.Id).Returns(true);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(reader.Id, readingClub.Id));

        Assert.Equal("Reader already has an active subscription for this reading club",
        exception.Message);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelSubscription_WhenSubscriptionExists()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());

        subscriptionRepository.GetByIdAsync(subscription.Id).Returns(subscription);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        await service.CancelAsync(subscription.Id);

        Assert.Equal(ClubSubscriptionStatus.Canceled, subscription.Status);
        Assert.NotNull(subscription.CanceledAt);

        await subscriptionRepository.Received(1).UpdateAsync(subscription);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowNotFound_WhenSubscriptionDoesNotExist()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var id = Guid.NewGuid();

        subscriptionRepository.GetByIdAsync(id).Returns((ClubSubscription?)null);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CancelAsync(id));

        Assert.Equal("Club subscription not found", exception.Message);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowConflict_WhenSubscriptionIsAlreadyCanceled()
    {
        var subscriptionRepository = Substitute.For<IClubSubscriptionRepository>();
        var readerRepository = Substitute.For<IReaderRepository>();
        var readingClubRepository = Substitute.For<IReadingClubRepository>();

        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());
        subscription.Cancel();

        subscriptionRepository.GetByIdAsync(subscription.Id).Returns(subscription);

        var service = new ClubSubscriptionService(
            subscriptionRepository,
            readerRepository,
            readingClubRepository);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CancelAsync(subscription.Id));

        Assert.Equal("Club subscription is already canceled", exception.Message);
    }
}

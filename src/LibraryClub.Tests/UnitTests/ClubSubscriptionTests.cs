using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;

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
    public void Restore_ShouldThrowException_WhenReaderIdIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.NewGuid(),
                Guid.Empty,
                Guid.NewGuid(),
                ClubSubscriptionStatus.Active,
                DateTime.UtcNow,
                canceledAt: null));

        Assert.Equal("Reader id cannot be empty", exception.Message);
    }

    [Fact]
    public void Restore_ShouldThrowException_WhenReadingClubIdIsEmpty()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ClubSubscription.Restore(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Empty,
                ClubSubscriptionStatus.Active,
                DateTime.UtcNow,
                canceledAt: null));

        Assert.Equal("Reading club id cannot be empty", exception.Message);
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

        Assert.Equal(
            "Club subscription cancellation date cannot be earlier than creation date",
            exception.Message);
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

        var exception = Assert.Throws<ConflictException>(subscription.Cancel);

        Assert.Equal("Club subscription is already canceled", exception.Message);
    }
}
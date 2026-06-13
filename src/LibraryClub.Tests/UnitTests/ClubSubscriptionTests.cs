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
    public void Cancel_ShouldSetStatusToCanceled_WhenSubscriptionIsActive()
    {
        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());

        subscription.Cancel();

        Assert.Equal(ClubSubscriptionStatus.Canceled, subscription.Status);
        Assert.NotNull(subscription.CanceledAt);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenSubscriptionIsAlreadyCanceled()
    {
        var subscription = new ClubSubscription(Guid.NewGuid(), Guid.NewGuid());
        subscription.Cancel();

        var exception = Assert.Throws<ConflictException>(() => subscription.Cancel());

        Assert.Equal("Club subscription is already canceled", exception.Message);
    }
}

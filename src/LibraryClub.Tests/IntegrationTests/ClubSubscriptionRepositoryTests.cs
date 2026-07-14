using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;
using LibraryClub.Tests.Fixtures;
using Microsoft.Data.SqlClient;

namespace LibraryClub.Tests.IntegrationTests;

[Trait("Category", "Integration")]
[Collection(IntegrationTestCollection.Name)]
public class ClubSubscriptionRepositoryTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly ReaderRepository _readerRepository = fixture.ReaderRepository;
    private readonly ReadingClubRepository _readingClubRepository = fixture.ReadingClubRepository;
    private readonly ClubSubscriptionRepository _subscriptionRepository = fixture.ClubSubscriptionRepository;

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_ShouldInsertSubscription_WhenSubscriptionIsValid()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("John Doe", "john@email.com");
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        await _readerRepository.AddAsync(reader, cancellationToken);
        await _readingClubRepository.AddAsync(readingClub, cancellationToken);

        var subscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        var savedSubscription = await _subscriptionRepository.GetByIdAsync(
            subscription.Id,
            cancellationToken);

        Assert.NotNull(savedSubscription);
        Assert.Equal(subscription.Id, savedSubscription.Id);
        Assert.Equal(reader.Id, savedSubscription.ReaderId);
        Assert.Equal(readingClub.Id, savedSubscription.ReadingClubId);
        Assert.Equal(ClubSubscriptionStatus.Active, savedSubscription.Status);
        Assert.Null(savedSubscription.CanceledAt);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSubscriptionDoesNotExist()
    {
        var cancellationToken = CancellationToken.None;

        var subscription = await _subscriptionRepository.GetByIdAsync(
            Guid.NewGuid(),
            cancellationToken);

        Assert.Null(subscription);
    }

    [Fact]
    public async Task ExistsActiveAsync_ShouldReturnTrue_WhenActiveSubscriptionExists()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Taylor Smith", "taylor@email.com");
        var readingClub = new ReadingClub("Science Club", null, "Sci-fi");

        await _readerRepository.AddAsync(reader, cancellationToken);
        await _readingClubRepository.AddAsync(readingClub, cancellationToken);

        var subscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        var exists = await _subscriptionRepository.ExistsActiveAsync(
            reader.Id,
            readingClub.Id,
            cancellationToken);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsActiveAsync_ShouldReturnFalse_WhenOnlyCanceledSubscriptionExists()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Arthur Morgan", "arthur@email.com");
        var readingClub = new ReadingClub("Adventure Club", null, "Adventure");

        await _readerRepository.AddAsync(reader, cancellationToken);
        await _readingClubRepository.AddAsync(readingClub, cancellationToken);

        var subscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        subscription.Cancel();

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        var exists = await _subscriptionRepository.ExistsActiveAsync(
            reader.Id,
            readingClub.Id,
            cancellationToken);

        Assert.False(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCancelSubscription_WhenSubscriptionExists()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Tiago Rodriguez", "rodriguez@email.com");
        var readingClub = new ReadingClub("Drama Club", null, "Drama");

        await _readerRepository.AddAsync(reader, cancellationToken);
        await _readingClubRepository.AddAsync(readingClub, cancellationToken);

        var subscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        subscription.Cancel();

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        var updatedSubscription = await _subscriptionRepository.GetByIdAsync(
            subscription.Id,
            cancellationToken);

        Assert.NotNull(updatedSubscription);
        Assert.Equal(ClubSubscriptionStatus.Canceled, updatedSubscription.Status);
        Assert.NotNull(updatedSubscription.CanceledAt);
    }

    [Fact]
    public async Task AddAsync_ShouldAllowNewSubscription_WhenPreviousSubscriptionIsCanceled()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Michael Marston", "michael@email.com");
        var readingClub = new ReadingClub("Suspense Club", null, "Suspense");

        await _readerRepository.AddAsync(reader, cancellationToken);
        await _readingClubRepository.AddAsync(readingClub, cancellationToken);

        var firstSubscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(firstSubscription, cancellationToken);

        firstSubscription.Cancel();

        await _subscriptionRepository.UpdateAsync(firstSubscription, cancellationToken);

        var secondSubscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(secondSubscription, cancellationToken);

        var savedSubscription = await _subscriptionRepository.GetByIdAsync(
            secondSubscription.Id,
            cancellationToken);

        Assert.NotNull(savedSubscription);
        Assert.Equal(ClubSubscriptionStatus.Active, savedSubscription.Status);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowSqlException_WhenActiveSubscriptionAlreadyExists()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Noah Smith", "noah@email.com");
        var readingClub = new ReadingClub("Romance Club", null, "Romance");

        await _readerRepository.AddAsync(reader, cancellationToken);
        await _readingClubRepository.AddAsync(readingClub, cancellationToken);

        var firstSubscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(firstSubscription, cancellationToken);

        var duplicateSubscription = new ClubSubscription(reader.Id, readingClub.Id);

        await Assert.ThrowsAsync<SqlException>(() =>
            _subscriptionRepository.AddAsync(duplicateSubscription, cancellationToken));
    }

    [Fact]
    public async Task GetByReaderAsync_ShouldReturnPagedSubscriptionsIncludingCanceled()
    {
        var cancellationToken = CancellationToken.None;

        var data = await CreateReaderWithSubscriptionsAsync(cancellationToken);

        var firstPage = await _subscriptionRepository.GetByReaderAsync(
            data.Reader.Id,
            page: 1,
            pageSize: 2,
            cancellationToken);

        var secondPage = await _subscriptionRepository.GetByReaderAsync(
            data.Reader.Id,
            page: 2,
            pageSize: 2,
            cancellationToken);

        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(2, firstPage.Items.Count);

        Assert.Equal(data.Subscriptions[2].Id, firstPage.Items[0].Id);
        Assert.Equal(data.Subscriptions[1].Id, firstPage.Items[1].Id);
        Assert.Contains(
            firstPage.Items,
            subscription => subscription.Status == ClubSubscriptionStatus.Canceled);

        Assert.Equal(2, secondPage.Page);
        Assert.Equal(2, secondPage.PageSize);

        var subscription = Assert.Single(secondPage.Items);
        Assert.Equal(data.Subscriptions[0].Id, subscription.Id);
    }

    [Fact]
    public async Task GetByReaderAsync_ShouldReturnEmptyPage_WhenReaderHasNoSubscriptions()
    {
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Pedro Silva", "pedro.silva@email.com");

        await _readerRepository.AddAsync(reader, cancellationToken);

        var result = await _subscriptionRepository.GetByReaderAsync(
            reader.Id,
            page: 1,
            pageSize: 10,
            cancellationToken);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetByReadingClubAsync_ShouldReturnPagedSubscriptionsIncludingCanceled()
    {
        var cancellationToken = CancellationToken.None;

        var data = await CreateReadingClubWithSubscriptionsAsync(cancellationToken);

        var firstPage = await _subscriptionRepository.GetByReadingClubAsync(
            data.ReadingClub.Id,
            page: 1,
            pageSize: 2,
            cancellationToken);

        var secondPage = await _subscriptionRepository.GetByReadingClubAsync(
            data.ReadingClub.Id,
            page: 2,
            pageSize: 2,
            cancellationToken);

        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(2, firstPage.Items.Count);

        Assert.Equal(data.Subscriptions[2].Id, firstPage.Items[0].Id);
        Assert.Equal(data.Subscriptions[1].Id, firstPage.Items[1].Id);
        Assert.Contains(
            firstPage.Items,
            subscription => subscription.Status == ClubSubscriptionStatus.Canceled);

        Assert.Equal(2, secondPage.Page);
        Assert.Equal(2, secondPage.PageSize);

        var subscription = Assert.Single(secondPage.Items);
        Assert.Equal(data.Subscriptions[0].Id, subscription.Id);
    }

    [Fact]
    public async Task GetByReadingClubAsync_ShouldReturnEmptyPage_WhenReadingClubHasNoSubscriptions()
    {
        var cancellationToken = CancellationToken.None;

        var readingClub = new ReadingClub("Harry Potter Club", null, "Fantasy");

        await _readingClubRepository.AddAsync(readingClub, cancellationToken);

        var result = await _subscriptionRepository.GetByReadingClubAsync(
            readingClub.Id,
            page: 1,
            pageSize: 10,
            cancellationToken);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    private async Task<(Reader Reader, List<ClubSubscription> Subscriptions)>
        CreateReaderWithSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var createdAt = DateTime.UtcNow;

        var reader = Reader.Restore(
            Guid.NewGuid(),
            "Taylor Smith",
            $"{Guid.NewGuid():N}@email.com",
            ReaderStatus.Active,
            createdAt.AddMinutes(-10));

        var firstClub = ReadingClub.Restore(
            Guid.NewGuid(),
            "Drama Club 1",
            null,
            "Drama",
            ReadingClubStatus.Active,
            createdAt.AddMinutes(-9));

        var secondClub = ReadingClub.Restore(
            Guid.NewGuid(),
            "Drama Club 2",
            null,
            "Drama",
            ReadingClubStatus.Active,
            createdAt.AddMinutes(-8));

        var thirdClub = ReadingClub.Restore(
            Guid.NewGuid(),
            "Drama Club 3",
            null,
            "Drama",
            ReadingClubStatus.Active,
            createdAt.AddMinutes(-7));

        var firstSubscription = ClubSubscription.Restore(
            Guid.NewGuid(),
            reader.Id,
            firstClub.Id,
            ClubSubscriptionStatus.Active,
            createdAt.AddMinutes(-6),
            canceledAt: null);

        var secondSubscription = ClubSubscription.Restore(
            Guid.NewGuid(),
            reader.Id,
            secondClub.Id,
            ClubSubscriptionStatus.Canceled,
            createdAt.AddMinutes(-5),
            createdAt.AddMinutes(-4));

        var thirdSubscription = ClubSubscription.Restore(
            Guid.NewGuid(),
            reader.Id,
            thirdClub.Id,
            ClubSubscriptionStatus.Active,
            createdAt.AddMinutes(-3),
            canceledAt: null);

        await _readerRepository.AddAsync(reader, cancellationToken);

        await _readingClubRepository.AddAsync(firstClub, cancellationToken);
        await _readingClubRepository.AddAsync(secondClub, cancellationToken);
        await _readingClubRepository.AddAsync(thirdClub, cancellationToken);

        await _subscriptionRepository.AddAsync(firstSubscription, cancellationToken);
        await _subscriptionRepository.AddAsync(secondSubscription, cancellationToken);
        await _subscriptionRepository.AddAsync(thirdSubscription, cancellationToken);

        return (
            reader,
            new List<ClubSubscription>
            {
                firstSubscription,
                secondSubscription,
                thirdSubscription
            });
    }

    private async Task<(ReadingClub ReadingClub, List<ClubSubscription> Subscriptions)>
        CreateReadingClubWithSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var createdAt = DateTime.UtcNow;

        var readingClub = ReadingClub.Restore(
            Guid.NewGuid(),
            "Romance Club",
            null,
            "Romance",
            ReadingClubStatus.Active,
            createdAt.AddMinutes(-10));

        var firstReader = Reader.Restore(
            Guid.NewGuid(),
            "Reader 1",
            $"reader.1.{Guid.NewGuid():N}@email.com",
            ReaderStatus.Active,
            createdAt.AddMinutes(-9));

        var secondReader = Reader.Restore(
            Guid.NewGuid(),
            "Reader 2",
            $"reader.2.{Guid.NewGuid():N}@email.com",
            ReaderStatus.Active,
            createdAt.AddMinutes(-8));

        var thirdReader = Reader.Restore(
            Guid.NewGuid(),
            "Reader 3",
            $"reader.3.{Guid.NewGuid():N}@email.com",
            ReaderStatus.Active,
            createdAt.AddMinutes(-7));

        var firstSubscription = ClubSubscription.Restore(
            Guid.NewGuid(),
            firstReader.Id,
            readingClub.Id,
            ClubSubscriptionStatus.Active,
            createdAt.AddMinutes(-6),
            canceledAt: null);

        var secondSubscription = ClubSubscription.Restore(
            Guid.NewGuid(),
            secondReader.Id,
            readingClub.Id,
            ClubSubscriptionStatus.Canceled,
            createdAt.AddMinutes(-5),
            createdAt.AddMinutes(-4));

        var thirdSubscription = ClubSubscription.Restore(
            Guid.NewGuid(),
            thirdReader.Id,
            readingClub.Id,
            ClubSubscriptionStatus.Active,
            createdAt.AddMinutes(-3),
            canceledAt: null);

        await _readingClubRepository.AddAsync(readingClub, cancellationToken);

        await _readerRepository.AddAsync(firstReader, cancellationToken);
        await _readerRepository.AddAsync(secondReader, cancellationToken);
        await _readerRepository.AddAsync(thirdReader, cancellationToken);

        await _subscriptionRepository.AddAsync(firstSubscription, cancellationToken);
        await _subscriptionRepository.AddAsync(secondSubscription, cancellationToken);
        await _subscriptionRepository.AddAsync(thirdSubscription, cancellationToken);

        return (
            readingClub,
            new List<ClubSubscription>
            {
                firstSubscription,
                secondSubscription,
                thirdSubscription
            });
    }
}
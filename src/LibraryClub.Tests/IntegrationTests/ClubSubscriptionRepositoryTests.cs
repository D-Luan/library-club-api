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
        var reader = new Reader("John Doe", "john@email.com");
        var readingClub = new ReadingClub("Fantasy Club", null, "Fantasy");

        await _readerRepository.AddAsync(reader);
        await _readingClubRepository.AddAsync(readingClub);

        var subscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(subscription);

        var savedSubscription = await _subscriptionRepository.GetByIdAsync(subscription.Id);

        Assert.NotNull(savedSubscription);
        Assert.Equal(subscription.Id, savedSubscription.Id);
        Assert.Equal(reader.Id, savedSubscription.ReaderId);
        Assert.Equal(readingClub.Id, savedSubscription.ReadingClubId);
        Assert.Equal(ClubSubscriptionStatus.Active, savedSubscription.Status);
        Assert.Null(savedSubscription.CanceledAt);
    }

    [Fact]
    public async Task ExistsActiveAsync_ShouldReturnTrue_WhenActiveSubscriptionExists()
    {
        var reader = new Reader("Taylor Smith", "taylor@email.com");
        var readingClub = new ReadingClub("Science Club", null, "sci-fi");

        await _readerRepository.AddAsync(reader);
        await _readingClubRepository.AddAsync(readingClub);

        await _subscriptionRepository.AddAsync(new ClubSubscription(reader.Id, readingClub.Id));

        var exists = await _subscriptionRepository.ExistsActiveAsync(reader.Id, readingClub.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCancelSubscription_WhenSubscriptionExists()
    {
        var reader = new Reader("Tiago Rodriguez", "rodriguez@email.com");
        var readingClub = new ReadingClub("Drama Club", null, "Drama");

        await _readerRepository.AddAsync(reader);
        await _readingClubRepository.AddAsync(readingClub);

        var subscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(subscription);

        subscription.Cancel();

        await _subscriptionRepository.UpdateAsync(subscription);

        var updatedSubscription = await _subscriptionRepository.GetByIdAsync(subscription.Id);

        Assert.NotNull(updatedSubscription);
        Assert.Equal(ClubSubscriptionStatus.Canceled, updatedSubscription.Status);
        Assert.NotNull(updatedSubscription.CanceledAt);
    }

    [Fact]
    public async Task AddAsync_ShouldAllowNewSubscription_WhenPreviousSubscriptionIsCanceled()
    {
        var reader = new Reader("Michael Marston", "michael@email.com");
        var readingClub = new ReadingClub("Suspense Club", null, "Suspense");

        await _readerRepository.AddAsync(reader);
        await _readingClubRepository.AddAsync(readingClub);

        var firstSubscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(firstSubscription);

        firstSubscription.Cancel();

        await _subscriptionRepository.UpdateAsync(firstSubscription);

        var secondSubscription = new ClubSubscription(reader.Id, readingClub.Id);

        await _subscriptionRepository.AddAsync(secondSubscription);

        var savedSubscription = await _subscriptionRepository.GetByIdAsync(secondSubscription.Id);

        Assert.NotNull(savedSubscription);
        Assert.Equal(ClubSubscriptionStatus.Active, savedSubscription.Status);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowSqlException_WhenActiveSubscriptionAlreadyExists()
    {
        var reader = new Reader("Noah Smith", "noah@email.com");
        var readingClub = new ReadingClub("Romance Club", null, "Romance");

        await _readerRepository.AddAsync(reader);
        await _readingClubRepository.AddAsync(readingClub);

        await _subscriptionRepository.AddAsync(new ClubSubscription(reader.Id, readingClub.Id));

        var duplicateSubscription = new ClubSubscription(reader.Id, readingClub.Id);

        await Assert.ThrowsAsync<SqlException>(() =>
            _subscriptionRepository.AddAsync(duplicateSubscription));
    }

    [Fact]
    public async Task GetByReaderAsync_ShouldReturnPagedSubscriptionsIncludingCanceled()
    {
        var data = await CreateReaderWithSubscriptionsAsync();

        var firstPage = await _subscriptionRepository.GetByReaderAsync(
            data.Reader.Id,
            page: 1,
            pageSize: 2);

        var secondPage = await _subscriptionRepository.GetByReaderAsync(
            data.Reader.Id,
            page: 2,
            pageSize: 2);

        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(data.Subscriptions[2].Id, firstPage.Items[0].Id);
        Assert.Equal(data.Subscriptions[1].Id, firstPage.Items[1].Id);
        Assert.Contains(firstPage.Items, subscription => subscription.Status == ClubSubscriptionStatus.Canceled);

        var subscription = Assert.Single(secondPage.Items);
        Assert.Equal(data.Subscriptions[0].Id, subscription.Id);
    }

    [Fact]
    public async Task GetByReaderAsync_ShouldReturnEmptyPage_WhenReaderHasNoSubscriptions()
    {
        var reader = new Reader("Pedro Silva", "pedro.silva@email.com");
        await _readerRepository.AddAsync(reader);

        var result = await _subscriptionRepository.GetByReaderAsync(reader.Id, 1, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task GetByReadingClubAsync_ShouldReturnPagedSubscriptionsIncludingCanceled()
    {
        var data = await CreateReadingClubWithSubscriptionsAsync();

        var firstPage = await _subscriptionRepository.GetByReadingClubAsync(
            data.ReadingClub.Id,
            page: 1,
            pageSize: 2);

        var secondPage = await _subscriptionRepository.GetByReadingClubAsync(
            data.ReadingClub.Id,
            page: 2,
            pageSize: 2);

        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(data.Subscriptions[2].Id, firstPage.Items[0].Id);
        Assert.Equal(data.Subscriptions[1].Id, firstPage.Items[1].Id);
        Assert.Contains(firstPage.Items, subscription => subscription.Status == ClubSubscriptionStatus.Canceled);

        var subscription = Assert.Single(secondPage.Items);
        Assert.Equal(data.Subscriptions[0].Id, subscription.Id);
    }

    [Fact]
    public async Task GetByReadingClubAsync_ShouldReturnEmptyPage_WhenReadingClubHasNoSubscriptions()
    {
        var readingClub = new ReadingClub("Harry Potter Club", null, "Fantasy");
        await _readingClubRepository.AddAsync(readingClub);

        var result = await _subscriptionRepository.GetByReadingClubAsync(readingClub.Id, 1, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    private async Task<(Reader Reader, List<ClubSubscription> Subscriptions)>
      CreateReaderWithSubscriptionsAsync()
    {
        var reader = new Reader("Taylor Smith", $"{Guid.NewGuid():N}@email.com");
        await _readerRepository.AddAsync(reader);

        var subscriptions = new List<ClubSubscription>();

        for (var index = 1; index <= 3; index++)
        {
            var readingClub = new ReadingClub($"Drama Club {index}", null, "Drama");

            await _readingClubRepository.AddAsync(readingClub);

            var subscription = new ClubSubscription(reader.Id, readingClub.Id);
            await _subscriptionRepository.AddAsync(subscription);

            if (index == 2)
            {
                subscription.Cancel();
                await _subscriptionRepository.UpdateAsync(subscription);
            }

            subscriptions.Add(subscription);

            await Task.Delay(10);
        }

        return (reader, subscriptions);
    }

    private async Task<(ReadingClub ReadingClub, List<ClubSubscription> Subscriptions)> CreateReadingClubWithSubscriptionsAsync()
    {
        var readingClub = new ReadingClub("Romance Club", null, "Romance");
        await _readingClubRepository.AddAsync(readingClub);

        var subscriptions = new List<ClubSubscription>();

        for (var index = 1; index <= 3; index++)
        {
            var reader = new Reader(
                $"Reader {index}",
                $"reader.{index}.{Guid.NewGuid():N}@email.com");

            await _readerRepository.AddAsync(reader);

            var subscription = new ClubSubscription(reader.Id, readingClub.Id);
            await _subscriptionRepository.AddAsync(subscription);

            if (index == 2)
            {
                subscription.Cancel();
                await _subscriptionRepository.UpdateAsync(subscription);
            }

            subscriptions.Add(subscription);

            await Task.Delay(10);
        }

        return (readingClub, subscriptions);
    }
}

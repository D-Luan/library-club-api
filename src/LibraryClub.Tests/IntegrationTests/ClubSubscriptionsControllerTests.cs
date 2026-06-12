using System.Net;
using System.Net.Http.Json;
using LibraryClub.Api.DTOs;
using LibraryClub.Tests.Fixtures;

namespace LibraryClub.Tests.IntegrationTests;

[Trait("Category", "Integration")]
[Collection(IntegrationTestCollection.Name)]
public class ClubSubscriptionsControllerTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly HttpClient _client = fixture.Client;
    public Task InitializeAsync() => fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        var reader = await CreateReaderAsync();
        var readingClub = await CreateReadingClubAsync();

        var request = new CreateClubSubscriptionRequest(reader.Id, readingClub.Id);

        var response = await _client.PostAsJsonAsync("/api/club-subscriptions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var subscription = await response.Content.ReadFromJsonAsync<ClubSubscriptionResponse>();

        Assert.NotNull(subscription);
        Assert.NotEqual(Guid.Empty, subscription.Id);
        Assert.Equal(reader.Id, subscription.ReaderId);
        Assert.Equal(readingClub.Id, subscription.ReadingClubId);
        Assert.Equal("Active", subscription.Status);
        Assert.Null(subscription.CanceledAt);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenReaderIdIsEmpty()
    {
        var readingClub = await CreateReadingClubAsync();

        var request = new CreateClubSubscriptionRequest(Guid.Empty, readingClub.Id);

        var response = await _client.PostAsJsonAsync("/api/club-subscriptions", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenReaderDoesNotExist()
    {
        var readingClub = await CreateReadingClubAsync();

        var request = new CreateClubSubscriptionRequest(Guid.NewGuid(), readingClub.Id);

        var response = await _client.PostAsJsonAsync("/api/club-subscriptions", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenReadingClubDoesNotExist()
    {
        var reader = await CreateReaderAsync();

        var request = new CreateClubSubscriptionRequest(reader.Id, Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/club-subscriptions", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenReaderIsInactive()
    {
        var reader = await CreateReaderAsync();
        var readingClub = await CreateReadingClubAsync();

        await _client.PatchAsync($"/api/readers/{reader.Id}/inactivate", content: null);

        var request = new CreateClubSubscriptionRequest(reader.Id, readingClub.Id);

        var response = await _client.PostAsJsonAsync("/api/club-subscriptions", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenReadingClubIsInactive()
    {
        var reader = await CreateReaderAsync();
        var readingClub = await CreateReadingClubAsync();

        await _client.PatchAsync($"/api/reading-clubs/{readingClub.Id}/inactivate", content: null);

        var request = new CreateClubSubscriptionRequest(reader.Id, readingClub.Id);

        var response = await _client.PostAsJsonAsync("/api/club-subscriptions", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenActiveSubscriptionAlreadyExists()
    {
        var reader = await CreateReaderAsync();
        var readingClub = await CreateReadingClubAsync();

        var request = new CreateClubSubscriptionRequest(reader.Id, readingClub.Id);

        await _client.PostAsJsonAsync("/api/club-subscriptions", request);

        var response = await _client.PostAsJsonAsync("/api/club-subscriptions", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenSubscriptionExists()
    {
        var reader = await CreateReaderAsync();
        var readingClub = await CreateReadingClubAsync();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/club-subscriptions",
            new CreateClubSubscriptionRequest(reader.Id, readingClub.Id));

        var createdSubscription = await
        createResponse.Content.ReadFromJsonAsync<ClubSubscriptionResponse>();

        var response = await _client.GetAsync($"/api/club-subscriptions/{createdSubscription!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var subscription = await response.Content.ReadFromJsonAsync<ClubSubscriptionResponse>();

        Assert.NotNull(subscription);
        Assert.Equal(createdSubscription.Id, subscription.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/club-subscriptions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNoContent_WhenSubscriptionExists()
    {
        var reader = await CreateReaderAsync();
        var readingClub = await CreateReadingClubAsync();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/club-subscriptions",
            new CreateClubSubscriptionRequest(reader.Id, readingClub.Id));

        var createdSubscription = await
        createResponse.Content.ReadFromJsonAsync<ClubSubscriptionResponse>();

        var cancelResponse = await _client.PatchAsync(
            $"/api/club-subscriptions/{createdSubscription!.Id}/cancel",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/club-subscriptions/{ createdSubscription.Id}");
        var subscription = await getResponse.Content.ReadFromJsonAsync<ClubSubscriptionResponse>();

        Assert.NotNull(subscription);
        Assert.Equal("Canceled", subscription.Status);
        Assert.NotNull(subscription.CanceledAt);
    }

    [Fact]
    public async Task Cancel_ShouldReturnConflict_WhenSubscriptionIsAlreadyCanceled()
    {
        var reader = await CreateReaderAsync();
        var readingClub = await CreateReadingClubAsync();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/club-subscriptions",
            new CreateClubSubscriptionRequest(reader.Id, readingClub.Id));

        var createdSubscription = await
        createResponse.Content.ReadFromJsonAsync<ClubSubscriptionResponse>();

        await _client.PatchAsync($"/api/club-subscriptions/{createdSubscription!.Id}/cancel",
        content: null);

        var response = await _client.PatchAsync(
            $"/api/club-subscriptions/{createdSubscription.Id}/cancel",
            content: null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
    {
        var response = await _client.PatchAsync(
            $"/api/club-subscriptions/{Guid.NewGuid()}/cancel",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<ReaderResponse> CreateReaderAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/readers",
            new CreateReaderRequest("John Marston", $"{Guid.NewGuid()}@email.com"));

        return (await response.Content.ReadFromJsonAsync<ReaderResponse>())!;
    }

    private async Task<ReadingClubResponse> CreateReadingClubAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/reading-clubs",
            new CreateReadingClubRequest("Fantasy Club", null, "Fantasy"));

        return (await response.Content.ReadFromJsonAsync<ReadingClubResponse>())!;
    }
}

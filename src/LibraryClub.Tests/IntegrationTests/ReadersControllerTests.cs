using System.Net;
using System.Net.Http.Json;
using LibraryClub.Api.DTOs;
using LibraryClub.Tests.Fixtures;

namespace LibraryClub.Tests.IntegrationTests;

[Trait("Category", "Integration")]
[Collection(IntegrationTestCollection.Name)]
public class ReadersControllerTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly HttpClient _client = fixture.Client;
    public Task InitializeAsync() => fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        var request = new CreateReaderRequest(
            "John Doe",
            $"john.{Guid.NewGuid():N}@email.com");

        var response = await _client.PostAsJsonAsync("/api/readers", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var reader = await response.Content.ReadFromJsonAsync<ReaderResponse>();

        Assert.NotNull(reader);
        Assert.NotEqual(Guid.Empty, reader.Id);
        Assert.Equal(request.Name, reader.Name);
        Assert.Equal(request.Email, reader.Email);
        Assert.Equal("Active", reader.Status);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        var request = new CreateReaderRequest("John Doe", "invalid-email");

        var response = await _client.PostAsJsonAsync("/api/readers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var email = $"mary.{Guid.NewGuid():N}@email.com";
        var request = new CreateReaderRequest("Mary Taylor", email);

        var firstResponse = await _client.PostAsJsonAsync("/api/readers", request);
        var secondResponse = await _client.PostAsJsonAsync("/api/readers", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenReaderExists()
    {
        var request = new CreateReaderRequest(
            "Robert Smith",
            $"robert.{Guid.NewGuid():N}@email.com");

        var createResponse = await _client.PostAsJsonAsync("/api/readers", request);
        var createdReader = await createResponse.Content.ReadFromJsonAsync<ReaderResponse>();

        var response = await _client.GetAsync($"/api/readers/{createdReader!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reader = await response.Content.ReadFromJsonAsync<ReaderResponse>();

        Assert.NotNull(reader);
        Assert.Equal(createdReader.Id, reader.Id);
        Assert.Equal(request.Name, reader.Name);
        Assert.Equal(request.Email, reader.Email);
        Assert.Equal("Active", reader.Status);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenReaderDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/readers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Inactivate_ShouldReturnNoContent_WhenReaderExists()
    {
        var request = new CreateReaderRequest(
            "Ana Lima",
            $"ana.{Guid.NewGuid():N}@email.com");

        var createResponse = await _client.PostAsJsonAsync("/api/readers", request);
        var createdReader = await createResponse.Content.ReadFromJsonAsync<ReaderResponse>();

        var inactivateResponse = await _client.PatchAsync(
            $"/api/readers/{createdReader!.Id}/inactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, inactivateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/readers/{createdReader.Id}");
        var reader = await getResponse.Content.ReadFromJsonAsync<ReaderResponse>();

        Assert.NotNull(reader);
        Assert.Equal("Inactive", reader.Status);
    }

    [Fact]
    public async Task Inactivate_ShouldReturnNotFound_WhenReaderDoesNotExist()
    {
        var response = await _client.PatchAsync(
            $"/api/readers/{Guid.NewGuid()}/inactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPagedReaders()
    {
        var firstReader = await CreateReaderAsync("Reader One");
        await Task.Delay(10);

        var secondReader = await CreateReaderAsync("Reader Two");
        await Task.Delay(10);

        var thirdReader = await CreateReaderAsync("Reader Three");

        var response = await _client.GetAsync("/api/readers?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ReaderResponse>>();

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(thirdReader.Id, result.Items[0].Id);
        Assert.Equal(secondReader.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithDefaultPagination()
    {
        await CreateReaderAsync("Reader One");

        var response = await _client.GetAsync("/api/readers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ReaderResponse>>();

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);

        var reader = Assert.Single(result.Items);
        Assert.Equal("Reader One", reader.Name);
    }

    [Fact]
    public async Task GetAll_ShouldReturnBadRequest_WhenPageIsInvalid()
    {
        var response = await _client.GetAsync("/api/readers?page=0&pageSize=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
    {
        var response = await _client.GetAsync("/api/readers?page=1&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnPagedSubscriptions_WhenReaderExists()
    {
        var reader = await CreateReaderAsync("Taylor Smith");

        var firstClub = await CreateReadingClubAsync("Romance Club");
        var firstSubscription = await CreateSubscriptionAsync(reader.Id, firstClub.Id);
        await Task.Delay(10);

        var secondClub = await CreateReadingClubAsync("Fantasy Club");
        var secondSubscription = await CreateSubscriptionAsync(reader.Id, secondClub.Id);

        var cancelResponse = await _client.PatchAsync(
            $"/api/club-subscriptions/{secondSubscription.Id}/cancel",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        await Task.Delay(10);

        var thirdClub = await CreateReadingClubAsync("Sci-fi Club");
        var thirdSubscription = await CreateSubscriptionAsync(reader.Id, thirdClub.Id);

        var firstPageResponse = await _client.GetAsync(
            $"/api/readers/{reader.Id}/subscriptions?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, firstPageResponse.StatusCode);

        var firstPage = await firstPageResponse.Content
            .ReadFromJsonAsync<PagedResponse<ClubSubscriptionResponse>>();

        Assert.NotNull(firstPage);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(2, firstPage.Items.Count);

        Assert.Equal(thirdSubscription.Id, firstPage.Items[0].Id);
        Assert.Equal(secondSubscription.Id, firstPage.Items[1].Id);
        Assert.Equal("Canceled", firstPage.Items[1].Status);
        Assert.NotNull(firstPage.Items[1].CanceledAt);

        var secondPageResponse = await _client.GetAsync(
            $"/api/readers/{reader.Id}/subscriptions?page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);

        var secondPage = await secondPageResponse.Content
            .ReadFromJsonAsync<PagedResponse<ClubSubscriptionResponse>>();

        Assert.NotNull(secondPage);
        Assert.Equal(3, secondPage.TotalCount);
        Assert.Equal(2, secondPage.TotalPages);

        var subscription = Assert.Single(secondPage.Items);
        Assert.Equal(firstSubscription.Id, subscription.Id);
        Assert.Equal("Active", subscription.Status);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnEmptyPage_WhenReaderHasNoSubscriptions()
    {
        var reader = await CreateReaderAsync("Mary Marston");

        var response = await _client.GetAsync($"/api/readers/{reader.Id}/subscriptions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<ClubSubscriptionResponse>>();

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnNotFound_WhenReaderDoesNotExist()
    {
        var response = await _client.GetAsync(
            $"/api/readers/{Guid.NewGuid()}/subscriptions?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnBadRequest_WhenPageIsInvalid()
    {
        var reader = await CreateReaderAsync("Pedro Silva");

        var response = await _client.GetAsync(
            $"/api/readers/{reader.Id}/subscriptions?page=0&pageSize=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
    {
        var reader = await CreateReaderAsync("Thiago Santana");

        var response = await _client.GetAsync(
            $"/api/readers/{reader.Id}/subscriptions?page=1&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnNoContent_WhenReaderIsInactive()
    {
        var reader = await CreateReaderAsync("Beatriz Nogueira");

        var inactivateResponse = await _client.PatchAsync(
            $"/api/readers/{reader.Id}/inactivate", content: null);

        Assert.Equal(HttpStatusCode.NoContent, inactivateResponse.StatusCode);

        var reactivateResponse = await _client.PatchAsync(
            $"/api/readers/{reader.Id}/reactivate", content: null);

        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/readers/{reader.Id}");
        var reactivatedReader = await getResponse.Content.ReadFromJsonAsync<ReaderResponse>();

        Assert.NotNull(reactivatedReader);
        Assert.Equal("Active", reactivatedReader.Status);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnNotFound_WhenReaderDoesNotExist()
    {
        var response = await _client.PatchAsync(
            $"/api/readers/{Guid.NewGuid()}/reactivate", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnConflict_WhenReaderIsAlreadyActive()
    {
        var reader = await CreateReaderAsync("Rafael Costa");

        var response = await _client.PatchAsync($"/api/readers/{reader.Id}/reactivate", content: null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<ReaderResponse> CreateReaderAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/readers",
            new CreateReaderRequest(name, $"{Guid.NewGuid():N}@email.com"));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ReaderResponse>())!;
    }

    private async Task<ReadingClubResponse> CreateReadingClubAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/reading-clubs",
            new CreateReadingClubRequest(name, "Test description", "Test genre"));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ReadingClubResponse>())!;
    }

    private async Task<ClubSubscriptionResponse> CreateSubscriptionAsync(Guid readerId, Guid readingClubId)
    {
        var response = await _client.PostAsJsonAsync("/api/club-subscriptions",
            new CreateClubSubscriptionRequest(readerId, readingClubId));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ClubSubscriptionResponse>())!;
    }
}

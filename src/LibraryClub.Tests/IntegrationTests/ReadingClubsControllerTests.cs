using System.Net;
using System.Net.Http.Json;
using LibraryClub.Api.DTOs;
using LibraryClub.Tests.Fixtures;

namespace LibraryClub.Tests.IntegrationTests;

[Trait("Category", "Integration")]
[Collection(IntegrationTestCollection.Name)]
public class ReadingClubsControllerTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly HttpClient _client = fixture.Client;

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        var request = new CreateReadingClubRequest(
            "Classic Books",
            "Monthly classics discussion",
            "Classics");

        var response = await _client.PostAsJsonAsync("/api/reading-clubs", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var readingClub = await response.Content.ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(readingClub);
        Assert.NotEqual(Guid.Empty, readingClub.Id);
        Assert.Equal(request.Name, readingClub.Name);
        Assert.Equal(request.Description, readingClub.Description);
        Assert.Equal(request.Genre, readingClub.Genre);
        Assert.Equal("Active", readingClub.Status);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDescriptionIsNull()
    {
        var request = new CreateReadingClubRequest(
            "Sci-Fi Club",
            null,
            "Science Fiction");

        var response = await _client.PostAsJsonAsync("/api/reading-clubs", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var readingClub = await response.Content.ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(readingClub);
        Assert.Null(readingClub.Description);
        Assert.Equal("Active", readingClub.Status);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        var request = new CreateReadingClubRequest(
            "",
            "Monthly discussion",
            "Classics");

        var response = await _client.PostAsJsonAsync("/api/reading-clubs", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameIsTooLong()
    {
        var request = new CreateReadingClubRequest(
            new string('A', 151),
            "Monthly discussion",
            "Classics");

        var response = await _client.PostAsJsonAsync("/api/reading-clubs", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenDescriptionIsTooLong()
    {
        var request = new CreateReadingClubRequest(
            "Classic Books",
            new string('A', 1001),
            "Classics");

        var response = await _client.PostAsJsonAsync("/api/reading-clubs", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenGenreIsEmpty()
    {
        var request = new CreateReadingClubRequest(
            "Classic Books",
            "Monthly discussion",
            "");

        var response = await _client.PostAsJsonAsync("/api/reading-clubs", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenGenreIsTooLong()
    {
        var request = new CreateReadingClubRequest(
            "Classic Books",
            "Monthly discussion",
            new string('A', 101));

        var response = await _client.PostAsJsonAsync("/api/reading-clubs", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenReadingClubExists()
    {
        var request = new CreateReadingClubRequest(
            "Fantasy Club",
            "Fantasy books",
            "Fantasy");

        var createResponse = await _client.PostAsJsonAsync("/api/reading-clubs", request);
        var createdReadingClub = await
        createResponse.Content.ReadFromJsonAsync<ReadingClubResponse>();

        var response = await _client.GetAsync($"/api/reading-clubs/{createdReadingClub!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var readingClub = await response.Content.ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(readingClub);
        Assert.Equal(createdReadingClub.Id, readingClub.Id);
        Assert.Equal(request.Name, readingClub.Name);
        Assert.Equal(request.Description, readingClub.Description);
        Assert.Equal(request.Genre, readingClub.Genre);
        Assert.Equal("Active", readingClub.Status);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenReadingClubDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/reading-clubs/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Inactivate_ShouldReturnNoContent_WhenReadingClubExists()
    {
        var request = new CreateReadingClubRequest(
            "History Club",
            "History books",
            "History");

        var createResponse = await _client.PostAsJsonAsync("/api/reading-clubs", request);
        var createdReadingClub = await
        createResponse.Content.ReadFromJsonAsync<ReadingClubResponse>();

        var inactivateResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{createdReadingClub!.Id}/inactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, inactivateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{createdReadingClub.Id}");
        var readingClub = await getResponse.Content.ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(readingClub);
        Assert.Equal("Inactive", readingClub.Status);
    }

    [Fact]
    public async Task Inactivate_ShouldReturnNotFound_WhenReadingClubDoesNotExist()
    {
        var response = await _client.PatchAsync(
            $"/api/reading-clubs/{Guid.NewGuid()}/inactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Archive_ShouldReturnNoContent_WhenReadingClubExists()
    {
        var request = new CreateReadingClubRequest(
            "Drama Club",
            "Drama books",
            "Drama");

        var createResponse = await _client.PostAsJsonAsync("/api/reading-clubs", request);
        var createdReadingClub = await
        createResponse.Content.ReadFromJsonAsync<ReadingClubResponse>();

        var archiveResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{createdReadingClub!.Id}/archive",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{createdReadingClub.Id}");
        var readingClub = await getResponse.Content.ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(readingClub);
        Assert.Equal("Archived", readingClub.Status);
    }

    [Fact]
    public async Task Archive_ShouldReturnNotFound_WhenReadingClubDoesNotExist()
    {
        var response = await _client.PatchAsync(
            $"/api/reading-clubs/{Guid.NewGuid()}/archive",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPagedReadingClubs()
    {
        var firstReadingClub = await CreateReadingClubAsync("Reading Club One");
        await Task.Delay(10);

        var secondReadingClub = await CreateReadingClubAsync("Reading Club Two");
        await Task.Delay(10);

        var thirdReadingClub = await CreateReadingClubAsync("Reading Club Three");

        var response = await _client.GetAsync("/api/reading-clubs?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ReadingClubResponse>>();

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(thirdReadingClub.Id, result.Items[0].Id);
        Assert.Equal(secondReadingClub.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithDefaultPagination()
    {
        await CreateReadingClubAsync("Reading Club One");

        var response = await _client.GetAsync("/api/reading-clubs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ReadingClubResponse>>();

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);

        var readingClub = Assert.Single(result.Items);
        Assert.Equal("Reading Club One", readingClub.Name);
    }

    [Fact]
    public async Task GetAll_ShouldReturnBadRequest_WhenPageIsInvalid()
    {
        var response = await _client.GetAsync("/api/reading-clubs?page=0&pageSize=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
    {
        var response = await _client.GetAsync("/api/reading-clubs?page=1&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnPagedSubscriptions_WhenReadingClubExists()
    {
        var readingClub = await CreateReadingClubAsync("Romance Club");

        var firstReader = await CreateReaderAsync("Thiago Santana");
        var firstSubscription = await CreateSubscriptionAsync(firstReader.Id, readingClub.Id);
        await Task.Delay(10);

        var secondReader = await CreateReaderAsync("John Smith");
        var secondSubscription = await CreateSubscriptionAsync(secondReader.Id, readingClub.Id);

        var cancelResponse = await _client.PatchAsync(
            $"/api/club-subscriptions/{secondSubscription.Id}/cancel",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        await Task.Delay(10);

        var thirdReader = await CreateReaderAsync("Mary Taylor");
        var thirdSubscription = await CreateSubscriptionAsync(thirdReader.Id, readingClub.Id);

        var firstPageResponse = await _client.GetAsync(
            $"/api/reading-clubs/{readingClub.Id}/subscriptions?page=1&pageSize=2");

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
            $"/api/reading-clubs/{readingClub.Id}/subscriptions?page=2&pageSize=2");

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
    public async Task GetSubscriptions_ShouldReturnEmptyPage_WhenReadingClubHasNoSubscriptions()
    {
        var readingClub = await CreateReadingClubAsync("Fantasy Club");

        var response = await _client.GetAsync($"/api/reading-clubs/{readingClub.Id}/subscriptions");

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
    public async Task GetSubscriptions_ShouldReturnNotFound_WhenReadingClubDoesNotExist()
    {
        var response = await _client.GetAsync(
            $"/api/reading-clubs/{Guid.NewGuid()}/subscriptions?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnBadRequest_WhenPageIsInvalid()
    {
        var readingClub = await CreateReadingClubAsync("Sci-fi Club");

        var response = await _client.GetAsync(
            $"/api/reading-clubs/{readingClub.Id}/subscriptions?page=0&pageSize=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
    {
        var readingClub = await CreateReadingClubAsync("Drama Club");

        var response = await _client.GetAsync(
            $"/api/reading-clubs/{readingClub.Id}/subscriptions?page=1&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContentAndPersistChanges_WhenRequestIsValid()
    {
        var readingClub = await CreateReadingClubAsync("Coastal Classics");

        var request = new UpdateReadingClubRequest(
            "Mystery Book Circle", 
            "Discussions about mystery novels",
            "Mystery");

        var updateResponse = await _client.PutAsJsonAsync($"/api/reading-clubs/{readingClub.Id}", request);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{readingClub.Id}");
        var updatedReadingClub = await getResponse.Content.ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(updatedReadingClub);
        Assert.Equal("Mystery Book Circle", updatedReadingClub.Name);
        Assert.Equal("Discussions about mystery novels", updatedReadingClub.Description);
        Assert.Equal("Mystery", updatedReadingClub.Genre);
        Assert.Equal("Active", updatedReadingClub.Status);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenReadingClubIsInactive()
    {
        var readingClub = await CreateReadingClubAsync("Science Fiction Society");

        var inactivateResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/inactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, inactivateResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/reading-clubs/{readingClub.Id}",
            new UpdateReadingClubRequest("Historical Fiction Forum", null, "Historical Fiction"));

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{readingClub.Id}");
        var updatedReadingClub = await getResponse.Content.ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(updatedReadingClub);
        Assert.Equal("Historical Fiction Forum", updatedReadingClub.Name);
        Assert.Null(updatedReadingClub.Description);
        Assert.Equal("Historical Fiction", updatedReadingClub.Genre);
        Assert.Equal("Inactive", updatedReadingClub.Status);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        var readingClub = await CreateReadingClubAsync("Validation Test Club");

        var validRequest = new UpdateReadingClubRequest(
            "Mystery Book Circle",
            "Discussions about mystery novels",
            "Mystery");

        var invalidRequests = new[]
        {
            validRequest with { Name = "" },
            validRequest with { Name = new string('A', 151) },
            validRequest with { Description = new string('A', 1001) },
            validRequest with { Genre = "" },
            validRequest with { Genre = new string('A', 101) }
        };

        foreach (var request in invalidRequests)
        {
            var response = await _client.PutAsJsonAsync(
                $"/api/reading-clubs/{readingClub.Id}",
                request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenReadingClubDoesNotExist()
    {
        var request = new UpdateReadingClubRequest(
            "Poetry Reading Circle",
            "Weekly readings of contemporary poetry",
            "Poetry");

        var response = await _client.PutAsJsonAsync($"/api/reading-clubs/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnConflict_WhenReadingClubIsArchived()
    {
        var readingClub = await CreateReadingClubAsync("Fantasy Book Guild");

        var archiveResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/archive",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var response = await _client.PutAsJsonAsync(
            $"/api/reading-clubs/{readingClub.Id}",
            new UpdateReadingClubRequest(
                "Literary Fiction Circle",
                "Discussions about contemporary literary fiction",
                "Literary Fiction"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<ReadingClubResponse> CreateReadingClubAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/reading-clubs",
            new CreateReadingClubRequest(name, "Test description", "Test genre"));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ReadingClubResponse>())!;
    }

    private async Task<ReaderResponse> CreateReaderAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/readers",
            new CreateReaderRequest(name, $"{Guid.NewGuid():N}@email.com"));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ReaderResponse>())!;
    }

    private async Task<ClubSubscriptionResponse> CreateSubscriptionAsync(Guid readerId, Guid readingClubId)
    {
        var response = await _client.PostAsJsonAsync("/api/club-subscriptions",
            new CreateClubSubscriptionRequest(readerId, readingClubId));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ClubSubscriptionResponse>())!;
    }
}

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
}

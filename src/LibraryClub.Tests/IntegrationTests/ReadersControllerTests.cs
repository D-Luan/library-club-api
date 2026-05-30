using System.Net;
using System.Net.Http.Json;
using LibraryClub.Api.DTOs;
using LibraryClub.Tests.Fixtures;

namespace LibraryClub.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class ReadersControllerTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly LibraryClubApiFactory _factory;
    private readonly HttpClient _client;

    public ReadersControllerTests(DatabaseFixture fixture)
    {
        _factory = new LibraryClubApiFactory(fixture.ConnectionString);
        _client = _factory.CreateClient();
    }

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

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
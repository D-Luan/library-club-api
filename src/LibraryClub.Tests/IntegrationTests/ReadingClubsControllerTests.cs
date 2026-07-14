using System.Net;
using System.Net.Http.Json;
using LibraryClub.Api.DTOs;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Models;
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
        var createdReadingClub = await createResponse.Content
            .ReadFromJsonAsync<ReadingClubResponse>();

        var response = await _client.GetAsync(
            $"/api/reading-clubs/{createdReadingClub!.Id}");

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
    public async Task GetAll_ShouldReturnOkWithPagedReadingClubs()
    {
        var readingClubs = await SeedReadingClubsAsync();

        var response = await _client.GetAsync("/api/reading-clubs?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<ReadingClubResponse>>();

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(readingClubs[2].Id, result.Items[0].Id);
        Assert.Equal(readingClubs[1].Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithDefaultPagination()
    {
        await CreateReadingClubAsync("Reading Club One");

        var response = await _client.GetAsync("/api/reading-clubs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<ReadingClubResponse>>();

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
    public async Task Update_ShouldReturnNoContentAndPersistChanges_WhenRequestIsValid()
    {
        var readingClub = await CreateReadingClubAsync("Coastal Classics");

        var request = new UpdateReadingClubRequest(
            "Mystery Book Circle",
            "Discussions about mystery novels",
            "Mystery");

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/reading-clubs/{readingClub.Id}",
            request);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{readingClub.Id}");
        var updatedReadingClub = await getResponse.Content
            .ReadFromJsonAsync<ReadingClubResponse>();

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

        var request = new UpdateReadingClubRequest(
            "Historical Fiction Forum",
            null,
            "Historical Fiction");

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/reading-clubs/{readingClub.Id}",
            request);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{readingClub.Id}");
        var updatedReadingClub = await getResponse.Content
            .ReadFromJsonAsync<ReadingClubResponse>();

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

        var response = await _client.PutAsJsonAsync(
            $"/api/reading-clubs/{Guid.NewGuid()}",
            request);

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

        var request = new UpdateReadingClubRequest(
            "Literary Fiction Circle",
            "Discussions about contemporary literary fiction",
            "Literary Fiction");

        var response = await _client.PutAsJsonAsync(
            $"/api/reading-clubs/{readingClub.Id}",
            request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Inactivate_ShouldReturnNoContent_WhenReadingClubExists()
    {
        var readingClub = await CreateReadingClubAsync("History Club");

        var inactivateResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/inactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, inactivateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{readingClub.Id}");
        var updatedReadingClub = await getResponse.Content
            .ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(updatedReadingClub);
        Assert.Equal("Inactive", updatedReadingClub.Status);
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
        var readingClub = await CreateReadingClubAsync("Drama Club");

        var archiveResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/archive",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{readingClub.Id}");
        var updatedReadingClub = await getResponse.Content
            .ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(updatedReadingClub);
        Assert.Equal("Archived", updatedReadingClub.Status);
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
    public async Task Reactivate_ShouldReturnNoContent_WhenReadingClubIsInactive()
    {
        var readingClub = await CreateReadingClubAsync("Historical Fiction Forum");

        var inactivateResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/inactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, inactivateResponse.StatusCode);

        var reactivateResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/reactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/reading-clubs/{readingClub.Id}");
        var reactivatedReadingClub = await getResponse.Content
            .ReadFromJsonAsync<ReadingClubResponse>();

        Assert.NotNull(reactivatedReadingClub);
        Assert.Equal("Active", reactivatedReadingClub.Status);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnNotFound_WhenReadingClubDoesNotExist()
    {
        var response = await _client.PatchAsync(
            $"/api/reading-clubs/{Guid.NewGuid()}/reactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnConflict_WhenReadingClubIsAlreadyActive()
    {
        var readingClub = await CreateReadingClubAsync("Mystery Book Circle");

        var response = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/reactivate",
            content: null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnConflict_WhenReadingClubIsArchived()
    {
        var readingClub = await CreateReadingClubAsync("Fantasy Book Guild");

        var archiveResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/archive",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var reactivateResponse = await _client.PatchAsync(
            $"/api/reading-clubs/{readingClub.Id}/reactivate",
            content: null);

        Assert.Equal(HttpStatusCode.Conflict, reactivateResponse.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnPagedSubscriptions_WhenReadingClubExists()
    {
        var seededData = await SeedReadingClubSubscriptionsAsync();

        var firstPageResponse = await _client.GetAsync(
            $"/api/reading-clubs/{seededData.ReadingClub.Id}/subscriptions?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, firstPageResponse.StatusCode);

        var firstPage = await firstPageResponse.Content
            .ReadFromJsonAsync<PagedResponse<ClubSubscriptionResponse>>();

        Assert.NotNull(firstPage);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(2, firstPage.Items.Count);

        Assert.Equal(seededData.ThirdSubscription.Id, firstPage.Items[0].Id);
        Assert.Equal(seededData.SecondSubscription.Id, firstPage.Items[1].Id);
        Assert.Equal("Canceled", firstPage.Items[1].Status);
        Assert.NotNull(firstPage.Items[1].CanceledAt);

        var secondPageResponse = await _client.GetAsync(
            $"/api/reading-clubs/{seededData.ReadingClub.Id}/subscriptions?page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);

        var secondPage = await secondPageResponse.Content
            .ReadFromJsonAsync<PagedResponse<ClubSubscriptionResponse>>();

        Assert.NotNull(secondPage);
        Assert.Equal(3, secondPage.TotalCount);
        Assert.Equal(2, secondPage.TotalPages);

        var subscription = Assert.Single(secondPage.Items);

        Assert.Equal(seededData.FirstSubscription.Id, subscription.Id);
        Assert.Equal("Active", subscription.Status);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnEmptyPage_WhenReadingClubHasNoSubscriptions()
    {
        var readingClub = await CreateReadingClubAsync("Fantasy Club");

        var response = await _client.GetAsync(
            $"/api/reading-clubs/{readingClub.Id}/subscriptions");

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

    private async Task<ReadingClubResponse> CreateReadingClubAsync(string name)
    {
        var request = new CreateReadingClubRequest(
            name,
            "Test description",
            "Test genre");

        var response = await _client.PostAsJsonAsync("/api/reading-clubs", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ReadingClubResponse>())!;
    }

    private async Task<List<ReadingClub>> SeedReadingClubsAsync()
    {
        var createdAt = DateTime.UtcNow;

        var readingClubs = new List<ReadingClub>
        {
            ReadingClub.Restore(
                Guid.NewGuid(),
                "Reading Club One",
                "Test description",
                "Test genre",
                ReadingClubStatus.Active,
                createdAt.AddMinutes(-3)),

            ReadingClub.Restore(
                Guid.NewGuid(),
                "Reading Club Two",
                "Test description",
                "Test genre",
                ReadingClubStatus.Active,
                createdAt.AddMinutes(-2)),

            ReadingClub.Restore(
                Guid.NewGuid(),
                "Reading Club Three",
                "Test description",
                "Test genre",
                ReadingClubStatus.Active,
                createdAt.AddMinutes(-1))
        };

        foreach (var readingClub in readingClubs)
        {
            await fixture.ReadingClubRepository.AddAsync(readingClub);
        }

        return readingClubs;
    }

    private async Task<ReadingClubSubscriptionsSeed> SeedReadingClubSubscriptionsAsync()
    {
        var createdAt = DateTime.UtcNow;

        var readingClub = ReadingClub.Restore(
            Guid.NewGuid(),
            "Romance Club",
            "Test description",
            "Test genre",
            ReadingClubStatus.Active,
            createdAt.AddMinutes(-10));

        var firstReader = Reader.Restore(
            Guid.NewGuid(),
            "Thiago Santana",
            $"thiago.{Guid.NewGuid():N}@email.com",
            ReaderStatus.Active,
            createdAt.AddMinutes(-9));

        var secondReader = Reader.Restore(
            Guid.NewGuid(),
            "John Smith",
            $"john.{Guid.NewGuid():N}@email.com",
            ReaderStatus.Active,
            createdAt.AddMinutes(-8));

        var thirdReader = Reader.Restore(
            Guid.NewGuid(),
            "Mary Taylor",
            $"mary.{Guid.NewGuid():N}@email.com",
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

        await fixture.ReadingClubRepository.AddAsync(readingClub);

        await fixture.ReaderRepository.AddAsync(firstReader);
        await fixture.ReaderRepository.AddAsync(secondReader);
        await fixture.ReaderRepository.AddAsync(thirdReader);

        await fixture.ClubSubscriptionRepository.AddAsync(firstSubscription);
        await fixture.ClubSubscriptionRepository.AddAsync(secondSubscription);
        await fixture.ClubSubscriptionRepository.AddAsync(thirdSubscription);

        return new ReadingClubSubscriptionsSeed(
            readingClub,
            firstSubscription,
            secondSubscription,
            thirdSubscription);
    }

    private sealed record ReadingClubSubscriptionsSeed(
        ReadingClub ReadingClub,
        ClubSubscription FirstSubscription,
        ClubSubscription SecondSubscription,
        ClubSubscription ThirdSubscription);
}
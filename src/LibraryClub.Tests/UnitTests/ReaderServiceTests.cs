using LibraryClub.Api.Common;
using LibraryClub.Api.Enums;
using LibraryClub.Api.Exceptions;
using LibraryClub.Api.Models;
using LibraryClub.Api.Repositories;
using LibraryClub.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LibraryClub.Tests.UnitTests;

[Trait("Category", "Unit")]
public class ReaderServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateReader_WhenEmailIsUnique()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        context.Repository
            .ExistsByEmailAsync("clara.bennett@email.com", cancellationToken)
            .Returns(false);

        var service = context.CreateService();

        var reader = await service.CreateAsync(
            "Clara Bennett",
            "clara.bennett@email.com",
            cancellationToken);

        Assert.NotEqual(Guid.Empty, reader.Id);
        Assert.Equal("Clara Bennett", reader.Name);
        Assert.Equal("clara.bennett@email.com", reader.Email);
        Assert.Equal(ReaderStatus.Active, reader.Status);

        await context.Repository.Received(1)
            .ExistsByEmailAsync("clara.bennett@email.com", cancellationToken);

        await context.Repository.Received(1).AddAsync(
            Arg.Is<Reader>(r =>
                r.Id == reader.Id &&
                r.Name == "Clara Bennett" &&
                r.Email == "clara.bennett@email.com" &&
                r.Status == ReaderStatus.Active),
            cancellationToken);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        context.Repository
            .ExistsByEmailAsync("diego.santos@email.com", cancellationToken)
            .Returns(true);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(
                "Diego Santos",
                "diego.santos@email.com",
                cancellationToken));

        Assert.Equal("Reader email already exists", exception.Message);

        await context.Repository.Received(1)
            .ExistsByEmailAsync("diego.santos@email.com", cancellationToken);

        await context.Repository.DidNotReceive().AddAsync(
            Arg.Any<Reader>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReader_WhenReaderExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Mary Taylor", "mary.taylor@email.com");

        context.Repository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        var service = context.CreateService();

        var result = await service.GetByIdAsync(reader.Id, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(reader.Id, result.Id);
        Assert.Equal("Mary Taylor", result.Name);

        await context.Repository.Received(1)
            .GetByIdAsync(reader.Id, cancellationToken);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReaderDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readerId = Guid.NewGuid();

        context.Repository
            .GetByIdAsync(readerId, cancellationToken)
            .Returns((Reader?)null);

        var service = context.CreateService();

        var result = await service.GetByIdAsync(readerId, cancellationToken);

        Assert.Null(result);

        await context.Repository.Received(1)
            .GetByIdAsync(readerId, cancellationToken);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedReaders()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readers = new List<Reader>
        {
            new("Beatriz Rocha", "beatriz.rocha@email.com"),
            new("Rafael Lima", "rafael.lima@email.com")
        };

        var pagedResult = new PagedResult<Reader>(
            readers,
            Page: 1,
            PageSize: 2,
            TotalCount: 3);

        context.Repository
            .GetPagedAsync(1, 2, cancellationToken)
            .Returns(pagedResult);

        var service = context.CreateService();

        var result = await service.GetPagedAsync(
            page: 1,
            pageSize: 2,
            cancellationToken);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        await context.Repository.Received(1)
            .GetPagedAsync(1, 2, cancellationToken);
    }

    [Fact]
    public async Task InactivateAsync_ShouldInactivateReader_WhenReaderExists()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Robert Smith", "robert.smith@email.com");

        context.Repository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        var service = context.CreateService();

        await service.InactivateAsync(reader.Id, cancellationToken);

        Assert.Equal(ReaderStatus.Inactive, reader.Status);

        await context.Repository.Received(1)
            .GetByIdAsync(reader.Id, cancellationToken);

        await context.Repository.Received(1).UpdateAsync(
            Arg.Is<Reader>(r =>
                r.Id == reader.Id &&
                r.Status == ReaderStatus.Inactive),
            cancellationToken);
    }

    [Fact]
    public async Task InactivateAsync_ShouldThrowNotFound_WhenReaderDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readerId = Guid.NewGuid();

        context.Repository
            .GetByIdAsync(readerId, cancellationToken)
            .Returns((Reader?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.InactivateAsync(readerId, cancellationToken));

        Assert.Equal("Reader not found", exception.Message);

        await context.Repository.Received(1)
            .GetByIdAsync(readerId, cancellationToken);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<Reader>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InactivateAsync_ShouldThrowConflict_WhenReaderIsAlreadyInactive()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Mary Taylor", "mary.taylor@email.com");
        reader.Inactivate();

        context.Repository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.InactivateAsync(reader.Id, cancellationToken));

        Assert.Equal("Reader is already inactive", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<Reader>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateReader_WhenReaderIsInactive()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Marina Alves", "marina.alves@email.com");
        reader.Inactivate();

        context.Repository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        var service = context.CreateService();

        await service.ReactivateAsync(reader.Id, cancellationToken);

        Assert.Equal(ReaderStatus.Active, reader.Status);

        await context.Repository.Received(1)
            .GetByIdAsync(reader.Id, cancellationToken);

        await context.Repository.Received(1).UpdateAsync(
            Arg.Is<Reader>(updatedReader =>
                updatedReader.Id == reader.Id &&
                updatedReader.Status == ReaderStatus.Active),
            cancellationToken);
    }

    [Fact]
    public async Task ReactivateAsync_ShouldThrowNotFound_WhenReaderDoesNotExist()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var readerId = Guid.NewGuid();

        context.Repository
            .GetByIdAsync(readerId, cancellationToken)
            .Returns((Reader?)null);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ReactivateAsync(readerId, cancellationToken));

        Assert.Equal("Reader not found", exception.Message);

        await context.Repository.Received(1)
            .GetByIdAsync(readerId, cancellationToken);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<Reader>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateAsync_ShouldThrowConflict_WhenReaderIsAlreadyActive()
    {
        var context = new TestContext();
        var cancellationToken = CancellationToken.None;

        var reader = new Reader("Lucas Ferreira", "lucas.ferreira@email.com");

        context.Repository
            .GetByIdAsync(reader.Id, cancellationToken)
            .Returns(reader);

        var service = context.CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.ReactivateAsync(reader.Id, cancellationToken));

        Assert.Equal("Reader is already active", exception.Message);

        await context.Repository.DidNotReceive().UpdateAsync(
            Arg.Any<Reader>(),
            Arg.Any<CancellationToken>());
    }

    private sealed class TestContext
    {
        public IReaderRepository Repository { get; } =
            Substitute.For<IReaderRepository>();

        public ReaderService CreateService()
        {
            return new ReaderService(
                Repository,
                NullLogger<ReaderService>.Instance);
        }
    }
}

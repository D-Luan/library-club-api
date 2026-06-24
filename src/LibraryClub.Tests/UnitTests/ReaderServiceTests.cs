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
        // Arrange
        var repository = Substitute.For<IReaderRepository>();

        repository
            .ExistsByEmailAsync("john.doe@email.com")
            .Returns(false);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        // Act
        var reader = await service.CreateAsync("John Doe", "john.doe@email.com");

        // Assert
        Assert.NotEqual(Guid.Empty, reader.Id);
        Assert.Equal("John Doe", reader.Name);
        Assert.Equal("john.doe@email.com", reader.Email);
        Assert.Equal(ReaderStatus.Active, reader.Status);

        await repository.Received(1).ExistsByEmailAsync("john.doe@email.com");
        await repository.Received(1).AddAsync(Arg.Is<Reader>(r =>
            r.Id == reader.Id &&
            r.Name == "John Doe" &&
            r.Email == "john.doe@email.com" &&
            r.Status == ReaderStatus.Active));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var repository = Substitute.For<IReaderRepository>();

        repository
            .ExistsByEmailAsync("john.doe@email.com")
            .Returns(true);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        // Act
        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync("John Doe", "john.doe@email.com"));

        // Assert
        Assert.Equal("Reader email already exists", exception.Message);

        await repository.Received(1).ExistsByEmailAsync("john.doe@email.com");
        await repository.DidNotReceive().AddAsync(Arg.Any<Reader>());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReader_WhenReaderExists()
    {
        // Arrange
        var repository = Substitute.For<IReaderRepository>();
        var reader = new Reader("Mary Taylor", "mary.taylor@email.com");

        repository
            .GetByIdAsync(reader.Id)
            .Returns(reader);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        // Act
        var result = await service.GetByIdAsync(reader.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reader.Id, result.Id);
        Assert.Equal("Mary Taylor", result.Name);

        await repository.Received(1).GetByIdAsync(reader.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReaderDoesNotExist()
    {
        // Arrange
        var repository = Substitute.For<IReaderRepository>();
        var readerId = Guid.NewGuid();

        repository
            .GetByIdAsync(readerId)
            .Returns((Reader?)null);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        // Act
        var result = await service.GetByIdAsync(readerId);

        // Assert
        Assert.Null(result);

        await repository.Received(1).GetByIdAsync(readerId);
    }

    [Fact]
    public async Task InactivateAsync_ShouldInactivateReader_WhenReaderExists()
    {
        // Arrange
        var repository = Substitute.For<IReaderRepository>();
        var reader = new Reader("Robert Smith", "robert.smith@email.com");

        repository
            .GetByIdAsync(reader.Id)
            .Returns(reader);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        // Act
        await service.InactivateAsync(reader.Id);

        // Assert
        Assert.Equal(ReaderStatus.Inactive, reader.Status);

        await repository.Received(1).GetByIdAsync(reader.Id);
        await repository.Received(1).UpdateAsync(Arg.Is<Reader>(r =>
            r.Id == reader.Id &&
            r.Status == ReaderStatus.Inactive));
    }

    [Fact]
    public async Task InactivateAsync_ShouldThrowException_WhenReaderDoesNotExist()
    {
        // Arrange
        var repository = Substitute.For<IReaderRepository>();
        var readerId = Guid.NewGuid();

        repository
            .GetByIdAsync(readerId)
            .Returns((Reader?)null);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.InactivateAsync(readerId));

        // Assert
        Assert.Equal("Reader not found", exception.Message);

        await repository.Received(1).GetByIdAsync(readerId);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Reader>());
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedReaders()
    {
        // Arrange
        var repository = Substitute.For<IReaderRepository>();

        var readers = new List<Reader>
        {
            new("Reader One", "reader.one@email.com"),
            new("Reader Two", "reader.two@email.com")
        };

        var pagedResult = new PagedResult<Reader>(
            readers,
            Page: 1,
            PageSize: 2,
            TotalCount: 3);

        repository.GetPagedAsync(1, 2).Returns(pagedResult);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        // Act
        var result = await service.GetPagedAsync(1, 2);

        // Assert
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        await repository.Received(1).GetPagedAsync(1, 2);
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateReader_WhenReaderIsInactive()
    {
        var repository = Substitute.For<IReaderRepository>();

        var reader = new Reader("Marina Alves", "marina.alves@email.com");

        reader.Inactivate();

        repository.GetByIdAsync(reader.Id).Returns(reader);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        await service.ReactivateAsync(reader.Id);

        Assert.Equal(ReaderStatus.Active, reader.Status);

        await repository.Received(1).GetByIdAsync(reader.Id);
        await repository.Received(1).UpdateAsync(Arg.Is<Reader>(updatedReader =>
            updatedReader.Id == reader.Id &&
            updatedReader.Status == ReaderStatus.Active));
    }

    [Fact]
    public async Task ReactivateAsync_ShouldThrowNotFoundException_WhenReaderDoesNotExist()
    {
        var repository = Substitute.For<IReaderRepository>();
        var readerId = Guid.NewGuid();

        repository.GetByIdAsync(readerId).Returns((Reader?)null);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ReactivateAsync(readerId));

        Assert.Equal("Reader not found", exception.Message);

        await repository.DidNotReceive().UpdateAsync(Arg.Any<Reader>());
    }

    [Fact]
    public async Task ReactivateAsync_ShouldThrowConflictException_WhenReaderIsAlreadyActive()
    {
        var repository = Substitute.For<IReaderRepository>();

        var reader = new Reader("Lucas Ferreira", "lucas.ferreira@email.com");

        repository.GetByIdAsync(reader.Id).Returns(reader);

        var service = new ReaderService(repository, NullLogger<ReaderService>.Instance);
        
        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.ReactivateAsync(reader.Id));

        Assert.Equal("Reader is already active", exception.Message);

        await repository.DidNotReceive().UpdateAsync(Arg.Any<Reader>());
    }
}

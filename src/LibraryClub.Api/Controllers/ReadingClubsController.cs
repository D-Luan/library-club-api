using FluentValidation;
using LibraryClub.Api.DTOs;
using LibraryClub.Api.Models;
using LibraryClub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryClub.Api.Controllers;

[ApiController]
[Route("api/reading-clubs")]
public sealed class ReadingClubsController(
    IReadingClubService readingClubService,
    IValidator<CreateReadingClubRequest> createReadingClubValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReadingClubResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadingClubResponse>> Create(CreateReadingClubRequest request)
    {
        var validationResult = await createReadingClubValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(error => new
            {
                error.PropertyName,
                error.ErrorMessage
            }));
        }

        var readingClub = await readingClubService.CreateAsync(
            request.Name,
            request.Description,
            request.Genre);

        return CreatedAtAction(
            nameof(GetById),
            new { id = readingClub.Id },
            MapToResponse(readingClub)
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReadingClubResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadingClubResponse>> GetById(Guid id)
    {
        var readingClub = await readingClubService.GetByIdAsync(id);

        if (readingClub is null) return NotFound();

        return Ok(MapToResponse(readingClub));
    }

    [HttpPatch("{id:guid}/inactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inactivate(Guid id)
    {
        await readingClubService.InactivateAsync(id);

        return NoContent();
    }

    [HttpPatch("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(Guid id)
    {
        await readingClubService.ArchiveAsync(id);

        return NoContent();
    }

    private static ReadingClubResponse MapToResponse(ReadingClub readingClub)
    {
        return new ReadingClubResponse(
            readingClub.Id,
            readingClub.Name,
            readingClub.Description,
            readingClub.Genre,
            readingClub.Status.ToString(),
            readingClub.CreatedAt
        );
    }
}

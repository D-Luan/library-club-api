using FluentValidation;
using LibraryClub.Api.DTOs;
using LibraryClub.Api.Models;
using LibraryClub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryClub.Api.Controllers;

[ApiController]
[Route("api/readers")]
public sealed class ReadersController(
    IReaderService readerService,
    IValidator<CreateReaderRequest> createReaderValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReaderResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReaderResponse>> Create(CreateReaderRequest request)
    {
        var validationResult = await createReaderValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(error => new
            {
                error.PropertyName,
                error.ErrorMessage
            }));
        }

        var reader = await readerService.CreateAsync(request.Name, request.Email);

        return CreatedAtAction(
            nameof(GetById),
            new { id = reader.Id },
            MapToResponse(reader)
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReaderResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReaderResponse>> GetById(Guid id)
    {
        var reader = await readerService.GetByIdAsync(id);

        if (reader is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(reader));
    }

    [HttpPatch("{id:guid}/inactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inactivate(Guid id)
    {
        await readerService.InactivateAsync(id);

        return NoContent();
    }

    private static ReaderResponse MapToResponse(Reader reader)
    {
        return new ReaderResponse(
            reader.Id,
            reader.Name,
            reader.Email,
            reader.Status.ToString(),
            reader.CreatedAt
        );
    }
}

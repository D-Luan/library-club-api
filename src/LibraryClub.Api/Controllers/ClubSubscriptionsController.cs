using FluentValidation;
using LibraryClub.Api.DTOs;
using LibraryClub.Api.Models;
using LibraryClub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryClub.Api.Controllers;

[ApiController]
[Route("api/club-subscriptions")]
public sealed class ClubSubscriptionsController(
    IClubSubscriptionService subscriptionService,
    IValidator<CreateClubSubscriptionRequest> createSubscriptionValidator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ClubSubscriptionResponse>> Create(CreateClubSubscriptionRequest
    request)
    {
        var validationResult = await createSubscriptionValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(error => new
            {
                error.PropertyName,
                error.ErrorMessage
            }));
        }

        var subscription = await subscriptionService.CreateAsync(
            request.ReaderId,
            request.ReadingClubId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = subscription.Id },
            MapToResponse(subscription));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClubSubscriptionResponse>> GetById(Guid id)
    {
        var subscription = await subscriptionService.GetByIdAsync(id);

        if (subscription is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(subscription));
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await subscriptionService.CancelAsync(id);

        return NoContent();
    }

    private static ClubSubscriptionResponse MapToResponse(ClubSubscription subscription)
    {
        return new ClubSubscriptionResponse(
            subscription.Id,
            subscription.ReaderId,
            subscription.ReadingClubId,
            subscription.Status.ToString(),
            subscription.CreatedAt,
            subscription.CanceledAt);
    }
}

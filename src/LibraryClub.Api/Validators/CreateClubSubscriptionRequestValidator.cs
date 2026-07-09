using FluentValidation;
using LibraryClub.Api.DTOs;

namespace LibraryClub.Api.Validators;

public sealed class CreateClubSubscriptionRequestValidator
    : AbstractValidator<CreateClubSubscriptionRequest>
{
    public CreateClubSubscriptionRequestValidator()
    {
        RuleFor(x => x.ReaderId)
            .NotEmpty()
            .WithMessage("Reader id is required");

        RuleFor(x => x.ReadingClubId)
            .NotEmpty()
            .WithMessage("Reading club id is required");
    }
}
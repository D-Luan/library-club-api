using FluentValidation;
using LibraryClub.Api.DTOs;

namespace LibraryClub.Api.Validators;

public class CreateReadingClubRequestValidator : AbstractValidator<CreateReadingClubRequest>
{
    public CreateReadingClubRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(150)
            .WithMessage("Name must have at most 150 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must have at most 1000 characters");

        RuleFor(x => x.Genre)
            .NotEmpty()
            .WithMessage("Genre is required")
            .MaximumLength(100)
            .WithMessage("Genre must have at most 100 characters");
    }
}
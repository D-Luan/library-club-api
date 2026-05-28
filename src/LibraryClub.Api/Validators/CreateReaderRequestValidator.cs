using FluentValidation;
using LibraryClub.Api.DTOs;

namespace LibraryClub.Api.Validators;

public class CreateReaderRequestValidator : AbstractValidator<CreateReaderRequest>
{
    public CreateReaderRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(150)
            .WithMessage("Name must have at most 150 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email format is invalid")
            .MaximumLength(255)
            .WithMessage("Email must have at most 255 characters");
    }
}
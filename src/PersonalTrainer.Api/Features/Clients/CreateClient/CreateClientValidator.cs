using FluentValidation;

namespace PersonalTrainer.Api.Features.Clients.CreateClient;

public sealed class CreateClientValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("DateOfBirth must be in the past.");
    }
}

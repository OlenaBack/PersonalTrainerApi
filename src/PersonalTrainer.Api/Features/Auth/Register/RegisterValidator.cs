using FluentValidation;
using PersonalTrainer.Api.Auth;

namespace PersonalTrainer.Api.Features.Auth.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => RoleNames.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");

        RuleFor(x => x.TrainerId)
            .NotNull()
            .When(x => x.Role == RoleNames.Client)
            .WithMessage("TrainerId is required when registering a client.");

        RuleFor(x => x.DateOfBirth)
            .NotNull()
            .When(x => x.Role == RoleNames.Client)
            .WithMessage("DateOfBirth is required when registering a client.");

        RuleFor(x => x.DateOfBirth!.Value)
            .Must(dateOfBirth => dateOfBirth < DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirth is not null)
            .WithMessage("DateOfBirth must be in the past.");
    }
}

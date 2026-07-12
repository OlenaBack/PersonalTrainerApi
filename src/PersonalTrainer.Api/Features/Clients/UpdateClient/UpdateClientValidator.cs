using FluentValidation;

namespace PersonalTrainer.Api.Features.Clients.UpdateClient;

public sealed class UpdateClientValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("DateOfBirth must be in the past.");
    }
}

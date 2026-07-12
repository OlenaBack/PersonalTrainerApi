using FluentValidation;

namespace PersonalTrainer.Api.Features.Sessions.UpdateSessionStatus;

public sealed class UpdateSessionStatusValidator : AbstractValidator<UpdateSessionStatusRequest>
{
    public UpdateSessionStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

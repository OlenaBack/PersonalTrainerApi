using FluentValidation;

namespace PersonalTrainer.Api.Features.Sessions.ScheduleSession;

public sealed class ScheduleSessionValidator : AbstractValidator<ScheduleSessionRequest>
{
    public ScheduleSessionValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ScheduledAtUtc)
            .Must(scheduledAtUtc => scheduledAtUtc > DateTime.UtcNow)
            .WithMessage("ScheduledAtUtc must be in the future.");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 480);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

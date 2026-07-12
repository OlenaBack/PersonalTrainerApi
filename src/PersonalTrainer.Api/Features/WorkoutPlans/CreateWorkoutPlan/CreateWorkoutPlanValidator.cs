using FluentValidation;

namespace PersonalTrainer.Api.Features.WorkoutPlans.CreateWorkoutPlan;

public sealed class CreateWorkoutPlanValidator : AbstractValidator<CreateWorkoutPlanRequest>
{
    public CreateWorkoutPlanValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate is not null)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

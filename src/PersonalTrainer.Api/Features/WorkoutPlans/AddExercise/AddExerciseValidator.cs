using FluentValidation;

namespace PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;

public sealed class AddExerciseValidator : AbstractValidator<AddExerciseRequest>
{
    public AddExerciseValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.Sets).GreaterThan(0);
        RuleFor(x => x.Reps).GreaterThan(0);
        RuleFor(x => x.WeightKg).GreaterThanOrEqualTo(0).When(x => x.WeightKg is not null);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}

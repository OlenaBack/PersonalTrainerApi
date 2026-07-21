using FluentValidation;

namespace PersonalTrainer.Api.Features.Exercises.CreateExercise;

public sealed class CreateExerciseValidator : AbstractValidator<CreateExerciseRequest>
{
    public CreateExerciseValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleForEach(x => x.Tags).NotEmpty().MaximumLength(50);
    }
}

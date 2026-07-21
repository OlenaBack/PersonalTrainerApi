using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.Exercises.CreateExercise;

public sealed class CreateExerciseHandler(
    AppDbContext dbContext,
    ICurrentTrainerAccessor currentTrainerAccessor,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<Result<ExerciseResponse>> HandleAsync(CreateExerciseRequest request, CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("Exercises.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            TrainerId = trainer.Id,
            Name = request.Name,
            Tags = request.Tags?.ToList() ?? [],
            CreatedAtUtc = dateTimeProvider.UtcNow,
        };

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ExerciseResponse(exercise.Id, exercise.Name, exercise.Tags, exercise.CreatedAtUtc);
    }
}

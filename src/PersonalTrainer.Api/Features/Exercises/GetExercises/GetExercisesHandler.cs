using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.Exercises.GetExercises;

public sealed class GetExercisesHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<IReadOnlyList<ExerciseResponse>>> HandleAsync(CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("Exercises.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var exercises = await dbContext.Exercises
            .Where(e => e.TrainerId == trainer.Id)
            .OrderBy(e => e.Name)
            .Select(e => new ExerciseResponse(e.Id, e.Name, e.Tags, e.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return exercises;
    }
}

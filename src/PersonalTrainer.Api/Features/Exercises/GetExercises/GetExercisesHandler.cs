using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.Exercises.GetExercises;

public sealed class GetExercisesHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<IReadOnlyList<ExerciseResponse>>> HandleAsync(GetExercisesRequest request, CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("Exercises.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var query = dbContext.Exercises.Where(e => e.TrainerId == trainer.Id);

        if (request.Tags is not null && request.Tags.Length > 0)
        {
            query = query.Where(e => request.Tags.All(tag =>
                e.Tags.Any(exerciseTag => exerciseTag.ToLower() == tag.ToLower())));
        }

        var exercises = await query
            .OrderBy(e => e.Name)
            .ThenBy(e => e.Id)
            .Select(e => new ExerciseResponse(e.Id, e.Name, e.Tags, e.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return exercises;
    }
}

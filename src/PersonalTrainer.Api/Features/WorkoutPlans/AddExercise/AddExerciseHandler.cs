using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;

public sealed class AddExerciseHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<ExerciseResponse>> HandleAsync(Guid planId, AddExerciseRequest request, CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("WorkoutPlans.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var planExists = await dbContext.WorkoutPlans.AnyAsync(w => w.Id == planId && w.TrainerId == trainer.Id, cancellationToken);
        if (!planExists)
        {
            return Error.NotFound("WorkoutPlans.NotFound", "Workout plan not found.");
        }

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            WorkoutPlanId = planId,
            Name = request.Name,
            Sets = request.Sets,
            Reps = request.Reps,
            WeightKg = request.WeightKg,
            Notes = request.Notes,
            OrderIndex = request.OrderIndex,
            Tags = request.Tags?.ToList() ?? [],
        };

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ExerciseResponse(exercise.Id, exercise.WorkoutPlanId, exercise.Name, exercise.Sets, exercise.Reps, exercise.WeightKg, exercise.Notes, exercise.OrderIndex, exercise.Tags);
    }
}

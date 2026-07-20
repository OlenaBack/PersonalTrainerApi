using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;

public sealed class AddExerciseHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<WorkoutPlanExerciseResponse>> HandleAsync(Guid planId, AddExerciseRequest request, CancellationToken cancellationToken)
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

        var exercise = await dbContext.Exercises.SingleOrDefaultAsync(
            e => e.Id == request.ExerciseId && e.TrainerId == trainer.Id, cancellationToken);
        if (exercise is null)
        {
            return Error.NotFound("WorkoutPlans.ExerciseNotFound", "Exercise not found.");
        }

        var planExercise = new WorkoutPlanExercise
        {
            Id = Guid.NewGuid(),
            WorkoutPlanId = planId,
            ExerciseId = exercise.Id,
            TrainerId = trainer.Id,
            Sets = request.Sets,
            Reps = request.Reps,
            WeightKg = request.WeightKg,
            Notes = request.Notes,
            OrderIndex = request.OrderIndex,
        };

        dbContext.WorkoutPlanExercises.Add(planExercise);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkoutPlanExerciseResponse(
            planExercise.Id,
            planExercise.WorkoutPlanId,
            exercise.Id,
            exercise.Name,
            exercise.Tags,
            planExercise.Sets,
            planExercise.Reps,
            planExercise.WeightKg,
            planExercise.Notes,
            planExercise.OrderIndex);
    }
}

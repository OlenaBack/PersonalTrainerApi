using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.WorkoutPlans.CreateWorkoutPlan;

public sealed class CreateWorkoutPlanHandler(
    AppDbContext dbContext,
    ICurrentTrainerAccessor currentTrainerAccessor,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<Result<WorkoutPlanResponse>> HandleAsync(Guid clientId, CreateWorkoutPlanRequest request, CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("WorkoutPlans.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var clientExists = await dbContext.Clients.AnyAsync(c => c.Id == clientId && c.TrainerId == trainer.Id, cancellationToken);
        if (!clientExists)
        {
            return Error.NotFound("WorkoutPlans.ClientNotFound", "Client not found.");
        }

        var plan = new WorkoutPlan
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            TrainerId = trainer.Id,
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAtUtc = dateTimeProvider.UtcNow,
        };

        dbContext.WorkoutPlans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkoutPlanResponse(plan.Id, plan.ClientId, plan.Title, plan.Description, plan.StartDate, plan.EndDate, plan.CreatedAtUtc);
    }
}

using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.WorkoutPlans.GetWorkoutPlans;

public sealed class GetWorkoutPlansHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<IReadOnlyList<WorkoutPlanResponse>>> HandleAsync(Guid clientId, CancellationToken cancellationToken)
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

        var plans = await dbContext.WorkoutPlans
            .Where(w => w.ClientId == clientId)
            .OrderByDescending(w => w.CreatedAtUtc)
            .Select(w => new WorkoutPlanResponse(w.Id, w.ClientId, w.Title, w.Description, w.StartDate, w.EndDate, w.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return plans;
    }
}

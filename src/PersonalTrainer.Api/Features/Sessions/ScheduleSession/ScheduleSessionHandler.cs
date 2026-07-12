using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.Sessions.ScheduleSession;

public sealed class ScheduleSessionHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<SessionResponse>> HandleAsync(ScheduleSessionRequest request, CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("Sessions.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var clientExists = await dbContext.Clients.AnyAsync(c => c.Id == request.ClientId && c.TrainerId == trainer.Id, cancellationToken);
        if (!clientExists)
        {
            return Error.NotFound("Sessions.ClientNotFound", "Client not found.");
        }

        if (request.WorkoutPlanId is not null)
        {
            var planBelongsToClient = await dbContext.WorkoutPlans.AnyAsync(
                w => w.Id == request.WorkoutPlanId && w.ClientId == request.ClientId, cancellationToken);

            if (!planBelongsToClient)
            {
                return Error.Validation("Sessions.WorkoutPlanMismatch", "The workout plan does not belong to the specified client.");
            }
        }

        var session = new Session
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            TrainerId = trainer.Id,
            WorkoutPlanId = request.WorkoutPlanId,
            ScheduledAtUtc = request.ScheduledAtUtc,
            DurationMinutes = request.DurationMinutes,
            Status = SessionStatus.Scheduled,
            Notes = request.Notes,
        };

        dbContext.Sessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new SessionResponse(session.Id, session.ClientId, session.WorkoutPlanId, session.ScheduledAtUtc, session.DurationMinutes, session.Status, session.Notes);
    }
}

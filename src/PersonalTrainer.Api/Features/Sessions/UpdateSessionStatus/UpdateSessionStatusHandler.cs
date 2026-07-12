using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.Sessions.UpdateSessionStatus;

public sealed class UpdateSessionStatusHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<SessionResponse>> HandleAsync(Guid sessionId, UpdateSessionStatusRequest request, CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("Sessions.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var session = await dbContext.Sessions
            .SingleOrDefaultAsync(s => s.Id == sessionId && s.TrainerId == trainer.Id, cancellationToken);

        if (session is null)
        {
            return Error.NotFound("Sessions.NotFound", "Session not found.");
        }

        session.Status = request.Status;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new SessionResponse(session.Id, session.ClientId, session.WorkoutPlanId, session.ScheduledAtUtc, session.DurationMinutes, session.Status, session.Notes);
    }
}

using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.Sessions.GetSessions;

public sealed class GetSessionsHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<IReadOnlyList<SessionResponse>>> HandleAsync(GetSessionsRequest request, CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("Sessions.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var query = dbContext.Sessions.Where(s => s.TrainerId == trainer.Id);

        if (request.From is not null)
        {
            query = query.Where(s => s.ScheduledAtUtc >= request.From);
        }

        if (request.To is not null)
        {
            query = query.Where(s => s.ScheduledAtUtc <= request.To);
        }

        if (request.Status is not null)
        {
            query = query.Where(s => s.Status == request.Status);
        }

        var sessions = await query
            .OrderBy(s => s.ScheduledAtUtc)
            .Select(s => new SessionResponse(s.Id, s.ClientId, s.WorkoutPlanId, s.ScheduledAtUtc, s.DurationMinutes, s.Status, s.Notes))
            .ToListAsync(cancellationToken);

        return sessions;
    }
}

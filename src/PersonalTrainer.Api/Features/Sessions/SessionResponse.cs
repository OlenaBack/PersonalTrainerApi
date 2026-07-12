using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.Sessions;

public sealed record SessionResponse(
    Guid Id,
    Guid ClientId,
    Guid? WorkoutPlanId,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    SessionStatus Status,
    string? Notes);

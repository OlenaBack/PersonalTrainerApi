namespace PersonalTrainer.Api.Features.Sessions.ScheduleSession;

public sealed record ScheduleSessionRequest(
    Guid ClientId,
    Guid? WorkoutPlanId,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string? Notes);

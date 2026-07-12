namespace PersonalTrainer.Api.Features.WorkoutPlans;

public sealed record WorkoutPlanResponse(
    Guid Id,
    Guid ClientId,
    string Title,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateTime CreatedAtUtc);

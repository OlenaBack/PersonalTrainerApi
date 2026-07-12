namespace PersonalTrainer.Api.Features.WorkoutPlans;

public sealed record ExerciseResponse(
    Guid Id,
    Guid WorkoutPlanId,
    string Name,
    int Sets,
    int Reps,
    decimal? WeightKg,
    string? Notes,
    int OrderIndex,
    IReadOnlyList<string> Tags);

namespace PersonalTrainer.Api.Features.WorkoutPlans;

public sealed record WorkoutPlanExerciseResponse(
    Guid Id,
    Guid WorkoutPlanId,
    Guid ExerciseId,
    string Name,
    IReadOnlyList<string> Tags,
    int Sets,
    int Reps,
    decimal? WeightKg,
    string? Notes,
    int OrderIndex);

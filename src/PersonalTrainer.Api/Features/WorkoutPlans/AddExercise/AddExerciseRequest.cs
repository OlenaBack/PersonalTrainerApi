namespace PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;

public sealed record AddExerciseRequest(
    string Name,
    int Sets,
    int Reps,
    decimal? WeightKg,
    string? Notes,
    int OrderIndex,
    IReadOnlyList<string>? Tags);

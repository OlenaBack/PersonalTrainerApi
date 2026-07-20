namespace PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;

public sealed record AddExerciseRequest(
    Guid ExerciseId,
    int Sets,
    int Reps,
    decimal? WeightKg,
    string? Notes,
    int OrderIndex);

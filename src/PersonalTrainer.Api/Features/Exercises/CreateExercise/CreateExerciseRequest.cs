namespace PersonalTrainer.Api.Features.Exercises.CreateExercise;

public sealed record CreateExerciseRequest(string Name, IReadOnlyList<string>? Tags);

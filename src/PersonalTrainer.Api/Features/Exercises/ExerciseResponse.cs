namespace PersonalTrainer.Api.Features.Exercises;

public sealed record ExerciseResponse(
    Guid Id,
    string Name,
    IReadOnlyList<string> Tags,
    DateTime CreatedAtUtc);

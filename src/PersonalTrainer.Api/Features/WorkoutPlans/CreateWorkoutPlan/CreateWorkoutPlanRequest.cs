namespace PersonalTrainer.Api.Features.WorkoutPlans.CreateWorkoutPlan;

public sealed record CreateWorkoutPlanRequest(string Title, string? Description, DateOnly StartDate, DateOnly? EndDate);

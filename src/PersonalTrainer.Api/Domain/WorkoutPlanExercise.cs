namespace PersonalTrainer.Api.Domain;

public sealed class WorkoutPlanExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutPlanId { get; set; }
    public Guid ExerciseId { get; set; }
    public Guid TrainerId { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }

    public WorkoutPlan WorkoutPlan { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}

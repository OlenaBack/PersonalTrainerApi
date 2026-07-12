namespace PersonalTrainer.Api.Domain;

public sealed class Exercise
{
    public Guid Id { get; set; }
    public Guid WorkoutPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }
    public List<string> Tags { get; set; } = [];

    public WorkoutPlan WorkoutPlan { get; set; } = null!;
}

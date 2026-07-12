namespace PersonalTrainer.Api.Domain;

public sealed class Session
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid TrainerId { get; set; }
    public Guid? WorkoutPlanId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public SessionStatus Status { get; set; }
    public string? Notes { get; set; }

    public Client Client { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public WorkoutPlan? WorkoutPlan { get; set; }
}

namespace PersonalTrainer.Api.Domain;

public sealed class WorkoutPlan
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid TrainerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Client Client { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public ICollection<WorkoutPlanExercise> Exercises { get; set; } = new List<WorkoutPlanExercise>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}

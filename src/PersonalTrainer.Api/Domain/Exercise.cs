namespace PersonalTrainer.Api.Domain;

public sealed class Exercise
{
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }

    public Trainer Trainer { get; set; } = null!;
}

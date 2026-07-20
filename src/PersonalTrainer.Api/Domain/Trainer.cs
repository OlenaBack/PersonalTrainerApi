namespace PersonalTrainer.Api.Domain;

public sealed class Trainer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}

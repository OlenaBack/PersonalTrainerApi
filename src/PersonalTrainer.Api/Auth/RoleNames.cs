namespace PersonalTrainer.Api.Auth;

public static class RoleNames
{
    public const string Trainer = "Trainer";
    public const string Client = "Client";

    public static readonly IReadOnlyCollection<string> All = [Trainer, Client];
}

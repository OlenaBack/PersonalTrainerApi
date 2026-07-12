namespace PersonalTrainer.Api.Features.Clients.CreateClient;

public sealed record CreateClientRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);

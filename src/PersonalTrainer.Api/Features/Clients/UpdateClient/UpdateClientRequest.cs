namespace PersonalTrainer.Api.Features.Clients.UpdateClient;

public sealed record UpdateClientRequest(string FirstName, string LastName, DateOnly DateOfBirth);

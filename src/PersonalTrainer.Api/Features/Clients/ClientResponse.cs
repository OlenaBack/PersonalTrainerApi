namespace PersonalTrainer.Api.Features.Clients;

public sealed record ClientResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    DateTime CreatedAtUtc);

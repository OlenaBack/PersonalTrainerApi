namespace PersonalTrainer.Api.Features.Clients.GetClients;

public sealed record GetClientsRequest(string? Search, DateOnly? BornAfter);

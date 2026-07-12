namespace PersonalTrainer.Api.Features.Auth.Register;

public sealed record RegisterResponse(Guid UserId, string Role, string AccessToken);

namespace PersonalTrainer.Api.Features.Auth.Login;

public sealed record LoginResponse(Guid UserId, string Role, string AccessToken);

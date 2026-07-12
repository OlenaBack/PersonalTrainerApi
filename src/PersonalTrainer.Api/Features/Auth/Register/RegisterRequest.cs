namespace PersonalTrainer.Api.Features.Auth.Register;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role,
    Guid? TrainerId,
    DateOnly? DateOfBirth);

using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Auth.Register;

public sealed class RegisterHandler(IUserProvisioningService provisioningService, IJwtTokenService jwtTokenService)
{
    public async Task<Result<RegisterResponse>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var provisionResult = request.Role == RoleNames.Trainer
            ? await provisioningService.CreateTrainerAsync(
                request.Email, request.Password, request.FirstName, request.LastName, cancellationToken)
            : await provisioningService.CreateClientAsync(
                request.Email, request.Password, request.FirstName, request.LastName,
                request.TrainerId!.Value, request.DateOfBirth!.Value, cancellationToken);

        if (provisionResult.IsFailure)
        {
            return Result<RegisterResponse>.Failure(provisionResult.Error!);
        }

        var accessToken = jwtTokenService.GenerateToken(provisionResult.Value, request.Role);

        return new RegisterResponse(provisionResult.Value.Id, request.Role, accessToken);
    }
}

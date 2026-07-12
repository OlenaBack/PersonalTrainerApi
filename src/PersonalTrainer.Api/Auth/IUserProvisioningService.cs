using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Auth;

public interface IUserProvisioningService
{
    Task<Result<ApplicationUser>> CreateTrainerAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken);

    Task<Result<ApplicationUser>> CreateClientAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        Guid trainerId,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken);
}

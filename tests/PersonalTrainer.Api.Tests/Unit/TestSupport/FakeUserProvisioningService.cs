using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Tests.Unit.TestSupport;

public sealed class FakeUserProvisioningService(
    Func<Result<ApplicationUser>> createTrainerResult,
    Func<Result<ApplicationUser>> createClientResult) : IUserProvisioningService
{
    public Guid? LastTrainerIdUsed { get; private set; }

    public Task<Result<ApplicationUser>> CreateTrainerAsync(
        string email, string password, string firstName, string lastName, CancellationToken cancellationToken)
        => Task.FromResult(createTrainerResult());

    public Task<Result<ApplicationUser>> CreateClientAsync(
        string email, string password, string firstName, string lastName,
        Guid trainerId, DateOnly dateOfBirth, CancellationToken cancellationToken)
    {
        LastTrainerIdUsed = trainerId;
        return Task.FromResult(createClientResult());
    }
}

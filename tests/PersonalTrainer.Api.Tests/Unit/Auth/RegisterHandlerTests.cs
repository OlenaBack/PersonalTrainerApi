using FluentAssertions;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Auth.Register;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Auth;

public class RegisterHandlerTests
{
    [Fact]
    public async Task HandleAsync_TrainerRole_ProvisionsTrainerAndReturnsToken()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "trainer@example.com" };
        var provisioning = new FakeUserProvisioningService(
            createTrainerResult: () => Result<ApplicationUser>.Success(user),
            createClientResult: () => throw new InvalidOperationException("should not be called"));
        var handler = new RegisterHandler(provisioning, new FakeJwtTokenService());

        var request = new RegisterRequest("trainer@example.com", "Password1", "Jane", "Doe", RoleNames.Trainer, null, null);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Role.Should().Be(RoleNames.Trainer);
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task HandleAsync_ClientRole_ProvisionsClientWithTrainerId()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "client@example.com" };
        var trainerId = Guid.NewGuid();
        var provisioning = new FakeUserProvisioningService(
            createTrainerResult: () => throw new InvalidOperationException("should not be called"),
            createClientResult: () => Result<ApplicationUser>.Success(user));
        var handler = new RegisterHandler(provisioning, new FakeJwtTokenService());

        var request = new RegisterRequest(
            "client@example.com", "Password1", "John", "Smith", RoleNames.Client,
            trainerId, new DateOnly(1990, 1, 1));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        provisioning.LastTrainerIdUsed.Should().Be(trainerId);
    }

    [Fact]
    public async Task HandleAsync_ProvisioningFails_PropagatesError()
    {
        var error = Error.Conflict("Users.EmailAlreadyRegistered", "already registered");
        var provisioning = new FakeUserProvisioningService(
            createTrainerResult: () => Result<ApplicationUser>.Failure(error),
            createClientResult: () => throw new InvalidOperationException("should not be called"));
        var handler = new RegisterHandler(provisioning, new FakeJwtTokenService());

        var request = new RegisterRequest("trainer@example.com", "Password1", "Jane", "Doe", RoleNames.Trainer, null, null);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}

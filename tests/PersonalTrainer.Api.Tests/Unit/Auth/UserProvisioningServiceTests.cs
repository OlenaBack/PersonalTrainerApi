using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Auth;

public class UserProvisioningServiceTests
{
    [Fact]
    public async Task CreateTrainerAsync_NewEmail_CreatesUserAndTrainer()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var service = new UserProvisioningService(harness.UserManager, harness.DbContext, new FakeDateTimeProvider(DateTime.UtcNow));

        var result = await service.CreateTrainerAsync("trainer@example.com", "Password1", "Jane", "Doe", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await harness.DbContext.Trainers.SingleOrDefaultAsync(t => t.UserId == result.Value.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTrainerAsync_DuplicateEmail_ReturnsConflict()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var service = new UserProvisioningService(harness.UserManager, harness.DbContext, new FakeDateTimeProvider(DateTime.UtcNow));
        await service.CreateTrainerAsync("trainer@example.com", "Password1", "Jane", "Doe", CancellationToken.None);

        var result = await service.CreateTrainerAsync("trainer@example.com", "Password1", "Jane", "Doe", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task CreateClientAsync_UnknownTrainer_ReturnsNotFound()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var service = new UserProvisioningService(harness.UserManager, harness.DbContext, new FakeDateTimeProvider(DateTime.UtcNow));

        var result = await service.CreateClientAsync(
            "client@example.com", "Password1", "John", "Smith", Guid.NewGuid(), new DateOnly(1990, 1, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateClientAsync_KnownTrainer_CreatesUserAndClient()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var service = new UserProvisioningService(harness.UserManager, harness.DbContext, new FakeDateTimeProvider(DateTime.UtcNow));
        var trainerResult = await service.CreateTrainerAsync("trainer@example.com", "Password1", "Jane", "Doe", CancellationToken.None);
        var trainer = await harness.DbContext.Trainers.SingleAsync(t => t.UserId == trainerResult.Value.Id);

        var result = await service.CreateClientAsync(
            "client@example.com", "Password1", "John", "Smith", trainer.Id, new DateOnly(1990, 1, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var client = await harness.DbContext.Clients.SingleOrDefaultAsync(c => c.UserId == result.Value.Id);
        client.Should().NotBeNull();
        client!.TrainerId.Should().Be(trainer.Id);
    }
}

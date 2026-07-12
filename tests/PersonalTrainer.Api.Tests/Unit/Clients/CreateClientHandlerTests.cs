using FluentAssertions;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Clients.CreateClient;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Clients;

public class CreateClientHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesClientUnderCurrentTrainer()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var provisioning = new UserProvisioningService(harness.UserManager, harness.DbContext, new FakeDateTimeProvider(DateTime.UtcNow));
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        harness.DbContext.Trainers.Add(trainer);
        await harness.DbContext.SaveChangesAsync();

        var handler = new CreateClientHandler(provisioning, new FakeCurrentTrainerAccessor(trainer), harness.DbContext);
        var request = new CreateClientRequest("client@example.com", "Password1", "John", "Smith", new DateOnly(1990, 1, 1));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task HandleAsync_NoTrainerProfile_ReturnsNotFound()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var provisioning = new UserProvisioningService(harness.UserManager, harness.DbContext, new FakeDateTimeProvider(DateTime.UtcNow));
        var handler = new CreateClientHandler(provisioning, new FakeCurrentTrainerAccessor(null), harness.DbContext);
        var request = new CreateClientRequest("client@example.com", "Password1", "John", "Smith", new DateOnly(1990, 1, 1));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}

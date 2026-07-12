using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Clients.DeleteClient;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Clients;

public class DeleteClientHandlerTests
{
    [Fact]
    public async Task HandleAsync_OwnClient_RemovesClientAndUser()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        harness.DbContext.Trainers.Add(trainer);
        await harness.DbContext.SaveChangesAsync();

        var user = new ApplicationUser { UserName = "client@example.com", Email = "client@example.com" };
        await harness.UserManager.CreateAsync(user, "Password1");

        var client = new Client { Id = Guid.NewGuid(), UserId = user.Id, TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        harness.DbContext.Clients.Add(client);
        await harness.DbContext.SaveChangesAsync();

        var handler = new DeleteClientHandler(harness.DbContext, harness.UserManager, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(client.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await harness.DbContext.Clients.FindAsync(client.Id)).Should().BeNull();
        (await harness.UserManager.FindByIdAsync(user.Id.ToString())).Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_UnknownClient_ReturnsNotFound()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        harness.DbContext.Trainers.Add(trainer);
        await harness.DbContext.SaveChangesAsync();

        var handler = new DeleteClientHandler(harness.DbContext, harness.UserManager, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

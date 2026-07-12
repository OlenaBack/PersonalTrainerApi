using FluentAssertions;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Clients.GetClients;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Clients;

public class GetClientsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsOnlyClientsOfCurrentTrainer()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var otherTrainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Bob", LastName = "Lee", CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.AddRange(trainer, otherTrainer);
        dbContext.Clients.AddRange(
            new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow },
            new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = otherTrainer.Id, FirstName = "Zed", LastName = "Zeta", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var handler = new GetClientsHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(new GetClientsRequest(null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(c => c.FirstName == "Alice");
    }

    [Fact]
    public async Task HandleAsync_FiltersBySearchTerm()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.AddRange(
            new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow },
            new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Bob", LastName = "Brown", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var handler = new GetClientsHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(new GetClientsRequest("ali", null), CancellationToken.None);

        result.Value.Should().ContainSingle(c => c.FirstName == "Alice");
    }
}

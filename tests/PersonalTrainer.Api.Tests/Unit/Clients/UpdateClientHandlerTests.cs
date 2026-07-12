using FluentAssertions;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Clients.UpdateClient;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Clients;

public class UpdateClientHandlerTests
{
    [Fact]
    public async Task HandleAsync_OwnClient_UpdatesFields()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.Add(client);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateClientHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));
        var request = new UpdateClientRequest("Alicia", "Adams-Ray", new DateOnly(1996, 2, 2));

        var result = await handler.HandleAsync(client.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("Alicia");
        result.Value.LastName.Should().Be("Adams-Ray");
    }

    [Fact]
    public async Task HandleAsync_UnknownClient_ReturnsNotFound()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateClientHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));
        var request = new UpdateClientRequest("Alicia", "Adams-Ray", new DateOnly(1996, 2, 2));

        var result = await handler.HandleAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}

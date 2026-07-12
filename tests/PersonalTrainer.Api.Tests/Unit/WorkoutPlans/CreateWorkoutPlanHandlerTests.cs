using FluentAssertions;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.WorkoutPlans.CreateWorkoutPlan;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.WorkoutPlans;

public class CreateWorkoutPlanHandlerTests
{
    [Fact]
    public async Task HandleAsync_OwnClient_CreatesPlan()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.Add(client);
        await dbContext.SaveChangesAsync();

        var handler = new CreateWorkoutPlanHandler(dbContext, new FakeCurrentTrainerAccessor(trainer), new FakeDateTimeProvider(DateTime.UtcNow));
        var request = new CreateWorkoutPlanRequest("Strength Basics", "8-week plan", new DateOnly(2026, 1, 1), null);

        var result = await handler.HandleAsync(client.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientId.Should().Be(client.Id);
    }

    [Fact]
    public async Task HandleAsync_ClientBelongsToOtherTrainer_ReturnsNotFound()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var otherTrainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Bob", LastName = "Lee", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = otherTrainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.AddRange(trainer, otherTrainer);
        dbContext.Clients.Add(client);
        await dbContext.SaveChangesAsync();

        var handler = new CreateWorkoutPlanHandler(dbContext, new FakeCurrentTrainerAccessor(trainer), new FakeDateTimeProvider(DateTime.UtcNow));
        var request = new CreateWorkoutPlanRequest("Strength Basics", "8-week plan", new DateOnly(2026, 1, 1), null);

        var result = await handler.HandleAsync(client.Id, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}

using FluentAssertions;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.WorkoutPlans.GetWorkoutPlans;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.WorkoutPlans;

public class GetWorkoutPlansHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPlansForClient()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.Add(client);
        dbContext.WorkoutPlans.Add(new WorkoutPlan
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            TrainerId = trainer.Id,
            Title = "Strength Basics",
            StartDate = new DateOnly(2026, 1, 1),
            CreatedAtUtc = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetWorkoutPlansHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(client.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(p => p.Title == "Strength Basics");
    }

    [Fact]
    public async Task HandleAsync_UnknownClient_ReturnsNotFound()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        await dbContext.SaveChangesAsync();

        var handler = new GetWorkoutPlansHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

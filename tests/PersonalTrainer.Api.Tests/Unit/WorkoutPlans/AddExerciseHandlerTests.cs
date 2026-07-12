using FluentAssertions;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.WorkoutPlans;

public class AddExerciseHandlerTests
{
    [Fact]
    public async Task HandleAsync_OwnPlan_AddsExercise()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        var plan = new WorkoutPlan { Id = Guid.NewGuid(), ClientId = client.Id, TrainerId = trainer.Id, Title = "Strength Basics", StartDate = new DateOnly(2026, 1, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.Add(client);
        dbContext.WorkoutPlans.Add(plan);
        await dbContext.SaveChangesAsync();

        var handler = new AddExerciseHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));
        var request = new AddExerciseRequest("Squat", 3, 10, 60m, null, 0);

        var result = await handler.HandleAsync(plan.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Squat");
    }

    [Fact]
    public async Task HandleAsync_PlanOwnedByOtherTrainer_ReturnsNotFound()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var otherTrainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Bob", LastName = "Lee", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = otherTrainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        var plan = new WorkoutPlan { Id = Guid.NewGuid(), ClientId = client.Id, TrainerId = otherTrainer.Id, Title = "Strength Basics", StartDate = new DateOnly(2026, 1, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.AddRange(trainer, otherTrainer);
        dbContext.Clients.Add(client);
        dbContext.WorkoutPlans.Add(plan);
        await dbContext.SaveChangesAsync();

        var handler = new AddExerciseHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));
        var request = new AddExerciseRequest("Squat", 3, 10, 60m, null, 0);

        var result = await handler.HandleAsync(plan.Id, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}

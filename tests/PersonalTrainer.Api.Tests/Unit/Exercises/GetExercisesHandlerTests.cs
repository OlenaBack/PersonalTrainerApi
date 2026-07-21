using FluentAssertions;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Exercises.GetExercises;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Exercises;

public class GetExercisesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsOwnExercisesOrderedByName()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Exercises.AddRange(
            new Exercise { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "Squat", CreatedAtUtc = DateTime.UtcNow },
            new Exercise { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "Bench Press", CreatedAtUtc = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var handler = new GetExercisesHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(e => e.Name).Should().Equal("Bench Press", "Squat");
    }

    [Fact]
    public async Task HandleAsync_ExcludesOtherTrainersExercises()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var otherTrainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Bob", LastName = "Lee", CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.AddRange(trainer, otherTrainer);
        dbContext.Exercises.Add(new Exercise { Id = Guid.NewGuid(), TrainerId = otherTrainer.Id, Name = "Deadlift", CreatedAtUtc = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var handler = new GetExercisesHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}

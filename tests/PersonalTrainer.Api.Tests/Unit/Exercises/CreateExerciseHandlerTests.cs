using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Exercises.CreateExercise;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Exercises;

public class CreateExerciseHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesExercise()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        await dbContext.SaveChangesAsync();

        var handler = new CreateExerciseHandler(dbContext, new FakeCurrentTrainerAccessor(trainer), new FakeDateTimeProvider(DateTime.UtcNow));
        var request = new CreateExerciseRequest("Squat", ["Legs", "Core"]);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Squat");
        result.Value.Tags.Should().BeEquivalentTo("Legs", "Core");
        (await dbContext.Exercises.CountAsync(e => e.TrainerId == trainer.Id)).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_NoTrainerProfile_ReturnsNotFound()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var handler = new CreateExerciseHandler(dbContext, new FakeCurrentTrainerAccessor(null), new FakeDateTimeProvider(DateTime.UtcNow));
        var request = new CreateExerciseRequest("Squat", null);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}

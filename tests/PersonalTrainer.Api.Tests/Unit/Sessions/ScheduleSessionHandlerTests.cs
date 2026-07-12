using FluentAssertions;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Sessions.ScheduleSession;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Sessions;

public class ScheduleSessionHandlerTests
{
    [Fact]
    public async Task HandleAsync_OwnClient_SchedulesSession()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.Add(client);
        await dbContext.SaveChangesAsync();

        var handler = new ScheduleSessionHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));
        var request = new ScheduleSessionRequest(client.Id, null, DateTime.UtcNow.AddDays(1), 60, "First session");

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SessionStatus.Scheduled);
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

        var handler = new ScheduleSessionHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));
        var request = new ScheduleSessionRequest(client.Id, null, DateTime.UtcNow.AddDays(1), 60, null);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_WorkoutPlanBelongsToDifferentClient_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        var otherClient = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Zed", LastName = "Zeta", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        var otherClientPlan = new WorkoutPlan { Id = Guid.NewGuid(), ClientId = otherClient.Id, TrainerId = trainer.Id, Title = "Other Plan", StartDate = new DateOnly(2026, 1, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.AddRange(client, otherClient);
        dbContext.WorkoutPlans.Add(otherClientPlan);
        await dbContext.SaveChangesAsync();

        var handler = new ScheduleSessionHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));
        var request = new ScheduleSessionRequest(client.Id, otherClientPlan.Id, DateTime.UtcNow.AddDays(1), 60, null);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
    }
}

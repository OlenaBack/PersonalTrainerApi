using FluentAssertions;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Sessions.UpdateSessionStatus;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Sessions;

public class UpdateSessionStatusHandlerTests
{
    [Fact]
    public async Task HandleAsync_OwnSession_UpdatesStatus()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        var session = new Session { Id = Guid.NewGuid(), ClientId = client.Id, TrainerId = trainer.Id, ScheduledAtUtc = DateTime.UtcNow.AddDays(1), DurationMinutes = 60, Status = SessionStatus.Scheduled };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.Add(client);
        dbContext.Sessions.Add(session);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateSessionStatusHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(session.Id, new UpdateSessionStatusRequest(SessionStatus.Completed), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact]
    public async Task HandleAsync_SessionOwnedByOtherTrainer_ReturnsNotFound()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var otherTrainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Bob", LastName = "Lee", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = otherTrainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        var session = new Session { Id = Guid.NewGuid(), ClientId = client.Id, TrainerId = otherTrainer.Id, ScheduledAtUtc = DateTime.UtcNow.AddDays(1), DurationMinutes = 60, Status = SessionStatus.Scheduled };
        dbContext.Trainers.AddRange(trainer, otherTrainer);
        dbContext.Clients.Add(client);
        dbContext.Sessions.Add(session);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateSessionStatusHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(session.Id, new UpdateSessionStatusRequest(SessionStatus.Completed), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}

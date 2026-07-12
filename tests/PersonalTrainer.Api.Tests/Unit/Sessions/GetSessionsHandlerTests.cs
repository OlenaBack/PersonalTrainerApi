using FluentAssertions;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Sessions.GetSessions;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Sessions;

public class GetSessionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_FiltersByStatus()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.Add(client);
        dbContext.Sessions.AddRange(
            new Session { Id = Guid.NewGuid(), ClientId = client.Id, TrainerId = trainer.Id, ScheduledAtUtc = DateTime.UtcNow.AddDays(1), DurationMinutes = 60, Status = SessionStatus.Scheduled },
            new Session { Id = Guid.NewGuid(), ClientId = client.Id, TrainerId = trainer.Id, ScheduledAtUtc = DateTime.UtcNow.AddDays(-1), DurationMinutes = 60, Status = SessionStatus.Completed });
        await dbContext.SaveChangesAsync();

        var handler = new GetSessionsHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(new GetSessionsRequest(null, null, SessionStatus.Completed), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(s => s.Status == SessionStatus.Completed);
    }

    [Fact]
    public async Task HandleAsync_FiltersByDateRange()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var trainer = new Trainer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", CreatedAtUtc = DateTime.UtcNow };
        var client = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TrainerId = trainer.Id, FirstName = "Alice", LastName = "Adams", DateOfBirth = new DateOnly(1995, 5, 1), CreatedAtUtc = DateTime.UtcNow };
        var inRange = new Session { Id = Guid.NewGuid(), ClientId = client.Id, TrainerId = trainer.Id, ScheduledAtUtc = DateTime.UtcNow.AddDays(2), DurationMinutes = 60, Status = SessionStatus.Scheduled };
        var outOfRange = new Session { Id = Guid.NewGuid(), ClientId = client.Id, TrainerId = trainer.Id, ScheduledAtUtc = DateTime.UtcNow.AddDays(10), DurationMinutes = 60, Status = SessionStatus.Scheduled };
        dbContext.Trainers.Add(trainer);
        dbContext.Clients.Add(client);
        dbContext.Sessions.AddRange(inRange, outOfRange);
        await dbContext.SaveChangesAsync();

        var handler = new GetSessionsHandler(dbContext, new FakeCurrentTrainerAccessor(trainer));

        var result = await handler.HandleAsync(
            new GetSessionsRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(5), null), CancellationToken.None);

        result.Value.Should().ContainSingle(s => s.Id == inRange.Id);
    }
}

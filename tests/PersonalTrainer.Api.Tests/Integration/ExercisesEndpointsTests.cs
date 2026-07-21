using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Exercises;
using PersonalTrainer.Api.Tests.Integration.TestSupport;
using System.Net;
using System.Net.Http.Json;

namespace PersonalTrainer.Api.Tests.Integration;

public class ExercisesEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetExercises_WithoutTags_ReturnsAllExercisesForTrainer()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-exercises-all@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        await SeedExercisesAsync(trainer.UserId,
            new Exercise { Name = "Bench Press", Tags = ["strength"] },
            new Exercise { Name = "Running", Tags = ["cardio"] });

        var response = await client.GetAsync("/api/exercises");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var exercises = await response.Content.ReadFromJsonAsync<List<ExerciseResponse>>(TestJsonOptions.Default);
        exercises.Should().BeEquivalentTo(
            [
                new { Name = "Bench Press", Tags = new[] { "strength" } },
                new { Name = "Running", Tags = new[] { "cardio" } },
            ],
            options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetExercises_WithMultipleTags_ReturnsOnlyExercisesContainingAllTagsCaseInsensitively()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-exercises-filter@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        await SeedExercisesAsync(trainer.UserId,
            new Exercise { Name = "Full Body Circuit", Tags = ["strength", "cardio"] },
            new Exercise { Name = "Bench Press", Tags = ["strength"] },
            new Exercise { Name = "Running", Tags = ["cardio"] });

        var response = await client.GetAsync("/api/exercises?tags=Strength&tags=Cardio");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var exercises = await response.Content.ReadFromJsonAsync<List<ExerciseResponse>>(TestJsonOptions.Default);
        exercises.Should().ContainSingle();
        exercises[0].Name.Should().Be("Full Body Circuit");
        exercises[0].Tags.Should().Equal("strength", "cardio");
    }

    [Fact]
    public async Task GetExercises_WithUnknownTag_ReturnsEmptyList()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-exercises-missing-tag@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        await SeedExercisesAsync(trainer.UserId,
            new Exercise { Name = "Bench Press", Tags = ["strength"] });

        var response = await client.GetAsync("/api/exercises?tags=Flexibility");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var exercises = await response.Content.ReadFromJsonAsync<List<ExerciseResponse>>(TestJsonOptions.Default);
        exercises.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetExercises_DoesNotReturnExercisesBelongingToAnotherTrainer()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-exercises-owner@example.com");
        var otherTrainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-exercises-other@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        await SeedExercisesAsync(trainer.UserId,
            new Exercise { Name = "Owner Circuit", Tags = ["strength", "cardio"] });
        await SeedExercisesAsync(otherTrainer.UserId,
            new Exercise { Name = "Other Trainer Circuit", Tags = ["strength", "cardio"] });

        var response = await client.GetAsync("/api/exercises?tags=Strength&tags=Cardio");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var exercises = await response.Content.ReadFromJsonAsync<List<ExerciseResponse>>(TestJsonOptions.Default);
        exercises.Should().ContainSingle();
        exercises[0].Name.Should().Be("Owner Circuit");
        exercises[0].Tags.Should().Equal("strength", "cardio");
    }

    private async Task SeedExercisesAsync(Guid userId, params Exercise[] exercises)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var trainer = await dbContext.Trainers.SingleAsync(t => t.UserId == userId);

        foreach (var exercise in exercises)
        {
            exercise.TrainerId = trainer.Id;
            dbContext.Exercises.Add(exercise);
        }

        await dbContext.SaveChangesAsync();
    }
}

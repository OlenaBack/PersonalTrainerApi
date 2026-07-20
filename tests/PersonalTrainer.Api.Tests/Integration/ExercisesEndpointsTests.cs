using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PersonalTrainer.Api.Features.Exercises;
using PersonalTrainer.Api.Features.Exercises.CreateExercise;
using PersonalTrainer.Api.Tests.Integration.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Integration;

public class ExercisesEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task CreateExercise_ThenGetExercises_ReturnsExercise()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-exercises-1@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        var createResponse = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest("Squat", ["Legs", "Core"]));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetAsync("/api/exercises");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var exercises = await listResponse.Content.ReadFromJsonAsync<List<ExerciseResponse>>();

        exercises.Should().ContainSingle(e => e.Name == "Squat" && e.Tags.Contains("Legs"));
    }
}

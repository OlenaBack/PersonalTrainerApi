using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PersonalTrainer.Api.Features.Clients;
using PersonalTrainer.Api.Features.Clients.CreateClient;
using PersonalTrainer.Api.Features.Exercises;
using PersonalTrainer.Api.Features.Exercises.CreateExercise;
using PersonalTrainer.Api.Features.WorkoutPlans;
using PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;
using PersonalTrainer.Api.Features.WorkoutPlans.CreateWorkoutPlan;
using PersonalTrainer.Api.Tests.Integration.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Integration;

public class WorkoutPlansEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task CreatePlan_AddExercise_ThenGetPlans_ReturnsPlan()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-plans-1@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        var clientResponse = await client.PostAsJsonAsync("/api/clients", new CreateClientRequest(
            "client-plans-1@example.com", AuthTestHelper.Password, "Ella", "Evans", new DateOnly(1995, 5, 1)));
        var createdClient = await clientResponse.Content.ReadFromJsonAsync<ClientResponse>();

        var planResponse = await client.PostAsJsonAsync($"/api/clients/{createdClient!.Id}/workout-plans", new CreateWorkoutPlanRequest(
            "Strength Basics", "8-week plan", new DateOnly(2026, 1, 1), null));
        planResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPlan = await planResponse.Content.ReadFromJsonAsync<WorkoutPlanResponse>();

        var catalogResponse = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest("Squat", ["Legs", "Core"]));
        catalogResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var catalogExercise = await catalogResponse.Content.ReadFromJsonAsync<ExerciseResponse>();

        var exerciseResponse = await client.PostAsJsonAsync($"/api/workout-plans/{createdPlan!.Id}/exercises", new AddExerciseRequest(
            catalogExercise!.Id, 3, 10, 60m, null, 0));
        exerciseResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetAsync($"/api/clients/{createdClient.Id}/workout-plans");
        var plans = await listResponse.Content.ReadFromJsonAsync<List<WorkoutPlanResponse>>();

        plans.Should().ContainSingle(p => p.Title == "Strength Basics");
    }
}

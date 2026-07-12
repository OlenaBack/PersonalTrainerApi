using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Clients;
using PersonalTrainer.Api.Features.Clients.CreateClient;
using PersonalTrainer.Api.Features.Sessions;
using PersonalTrainer.Api.Features.Sessions.ScheduleSession;
using PersonalTrainer.Api.Features.Sessions.UpdateSessionStatus;
using PersonalTrainer.Api.Tests.Integration.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Integration;

public class SessionsEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ScheduleSession_ThenUpdateStatus_ThenGetSessions_ReflectsChange()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-sessions-1@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        var clientResponse = await client.PostAsJsonAsync("/api/clients", new CreateClientRequest(
            "client-sessions-1@example.com", AuthTestHelper.Password, "Finn", "Foster", new DateOnly(1995, 5, 1)));
        var createdClient = await clientResponse.Content.ReadFromJsonAsync<ClientResponse>();

        var scheduleResponse = await client.PostAsJsonAsync("/api/sessions", new ScheduleSessionRequest(
            createdClient!.Id, null, DateTime.UtcNow.AddDays(1), 60, "First session"));
        scheduleResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await scheduleResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var updateResponse = await client.PatchAsJsonAsync($"/api/sessions/{session!.Id}/status", new UpdateSessionStatusRequest(SessionStatus.Completed));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await client.GetAsync("/api/sessions?status=Completed");
        var sessions = await listResponse.Content.ReadFromJsonAsync<List<SessionResponse>>();

        sessions.Should().ContainSingle(s => s.Id == session.Id && s.Status == SessionStatus.Completed);
    }
}

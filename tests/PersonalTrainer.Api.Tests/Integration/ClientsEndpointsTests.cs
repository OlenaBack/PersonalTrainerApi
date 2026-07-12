using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PersonalTrainer.Api.Features.Clients;
using PersonalTrainer.Api.Features.Clients.CreateClient;
using PersonalTrainer.Api.Tests.Integration.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Integration;

public class ClientsEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task CreateClient_ThenGetById_ReturnsClient()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-clients-1@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        var createResponse = await client.PostAsJsonAsync("/api/clients", new CreateClientRequest(
            "client-1@example.com", AuthTestHelper.Password, "Alice", "Adams", new DateOnly(1995, 5, 1)));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ClientResponse>();

        var getResponse = await client.GetAsync($"/api/clients/{created!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ClientResponse>();
        fetched!.FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetClients_FiltersBySearchTerm()
    {
        var client = factory.CreateClient();
        var trainer = await AuthTestHelper.RegisterTrainerAsync(client, "trainer-clients-2@example.com");
        AuthTestHelper.AuthorizeAs(client, trainer.AccessToken);

        await client.PostAsJsonAsync("/api/clients", new CreateClientRequest(
            "client-2a@example.com", AuthTestHelper.Password, "Bella", "Bishop", new DateOnly(1995, 5, 1)));
        await client.PostAsJsonAsync("/api/clients", new CreateClientRequest(
            "client-2b@example.com", AuthTestHelper.Password, "Carl", "Chapman", new DateOnly(1995, 5, 1)));

        var response = await client.GetAsync("/api/clients?search=bel");

        var clients = await response.Content.ReadFromJsonAsync<List<ClientResponse>>();
        clients.Should().ContainSingle(c => c.FirstName == "Bella");
    }

    [Fact]
    public async Task GetClientById_FromDifferentTrainer_ReturnsNotFound()
    {
        var ownerClient = factory.CreateClient();
        var owner = await AuthTestHelper.RegisterTrainerAsync(ownerClient, "trainer-clients-owner@example.com");
        AuthTestHelper.AuthorizeAs(ownerClient, owner.AccessToken);
        var createResponse = await ownerClient.PostAsJsonAsync("/api/clients", new CreateClientRequest(
            "client-owned@example.com", AuthTestHelper.Password, "Dana", "Diaz", new DateOnly(1995, 5, 1)));
        var created = await createResponse.Content.ReadFromJsonAsync<ClientResponse>();

        var intruderClient = factory.CreateClient();
        var intruder = await AuthTestHelper.RegisterTrainerAsync(intruderClient, "trainer-clients-intruder@example.com");
        AuthTestHelper.AuthorizeAs(intruderClient, intruder.AccessToken);

        var response = await intruderClient.GetAsync($"/api/clients/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

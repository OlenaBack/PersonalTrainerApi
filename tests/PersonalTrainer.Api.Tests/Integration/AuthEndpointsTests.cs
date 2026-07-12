using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Features.Auth.Login;
using PersonalTrainer.Api.Features.Auth.Register;
using PersonalTrainer.Api.Tests.Integration.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Integration;

public class AuthEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Register_NewTrainer_ReturnsCreatedWithToken()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "trainer1@example.com", AuthTestHelper.Password, "Jane", "Doe", RoleNames.Trainer, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Role.Should().Be(RoleNames.Trainer);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.RegisterTrainerAsync(client, "duplicate@example.com");

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "duplicate@example.com", AuthTestHelper.Password, "Jane", "Doe", RoleNames.Trainer, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.RegisterTrainerAsync(client, "login-success@example.com");

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("login-success@example.com", AuthTestHelper.Password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.RegisterTrainerAsync(client, "login-fail@example.com");

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("login-fail@example.com", "WrongPassword1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

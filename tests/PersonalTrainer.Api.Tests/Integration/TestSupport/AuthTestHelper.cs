using System.Net.Http.Headers;
using System.Net.Http.Json;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Features.Auth.Register;

namespace PersonalTrainer.Api.Tests.Integration.TestSupport;

public static class AuthTestHelper
{
    public const string Password = "Password1!";

    public static async Task<RegisterResponse> RegisterTrainerAsync(HttpClient client, string email)
    {
        var request = new RegisterRequest(email, Password, "Test", "Trainer", RoleNames.Trainer, null, null);
        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<RegisterResponse>())!;
    }

    public static void AuthorizeAs(HttpClient client, string accessToken)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
}

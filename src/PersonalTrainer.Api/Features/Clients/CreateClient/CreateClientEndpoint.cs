using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Clients.CreateClient;

internal static class CreateClientEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync)
            .WithName("CreateClient")
            .WithTags("Clients")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)
            .AddEndpointFilter<ValidationFilter<CreateClientRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        CreateClientRequest request,
        CreateClientHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(response => Results.Created($"/api/clients/{response.Id}", response));
    }
}

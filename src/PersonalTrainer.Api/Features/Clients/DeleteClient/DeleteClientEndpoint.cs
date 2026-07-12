using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Clients.DeleteClient;

internal static class DeleteClientEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{clientId:guid}", HandleAsync)
            .WithName("DeleteClient")
            .WithTags("Clients")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly);
    }

    private static async Task<IResult> HandleAsync(
        Guid clientId,
        DeleteClientHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(clientId, cancellationToken);
        return result.ToHttpResult(Results.NoContent);
    }
}

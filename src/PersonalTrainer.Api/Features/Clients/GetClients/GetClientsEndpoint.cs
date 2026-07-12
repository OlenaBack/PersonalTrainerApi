using Microsoft.AspNetCore.Mvc;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Clients.GetClients;

internal static class GetClientsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleAsync)
            .WithName("GetClients")
            .WithTags("Clients")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] GetClientsRequest request,
        GetClientsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(Results.Ok);
    }
}

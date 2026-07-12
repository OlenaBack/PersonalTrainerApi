using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Clients.GetClientById;

internal static class GetClientByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{clientId:guid}", HandleAsync)
            .WithName("GetClientById")
            .WithTags("Clients")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly);
    }

    private static async Task<IResult> HandleAsync(
        Guid clientId,
        GetClientByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(clientId, cancellationToken);
        return result.ToHttpResult(Results.Ok);
    }
}

using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Clients.UpdateClient;

internal static class UpdateClientEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{clientId:guid}", HandleAsync)
            .WithName("UpdateClient")
            .WithTags("Clients")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)
            .AddEndpointFilter<ValidationFilter<UpdateClientRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        Guid clientId,
        UpdateClientRequest request,
        UpdateClientHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(clientId, request, cancellationToken);
        return result.ToHttpResult(Results.Ok);
    }
}

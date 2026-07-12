using Microsoft.AspNetCore.Mvc;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Sessions.GetSessions;

internal static class GetSessionsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleAsync)
            .WithName("GetSessions")
            .WithTags("Sessions")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] GetSessionsRequest request,
        GetSessionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(Results.Ok);
    }
}

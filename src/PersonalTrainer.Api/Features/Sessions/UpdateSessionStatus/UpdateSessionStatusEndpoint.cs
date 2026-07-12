using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Sessions.UpdateSessionStatus;

internal static class UpdateSessionStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPatch("/{sessionId:guid}/status", HandleAsync)
            .WithName("UpdateSessionStatus")
            .WithTags("Sessions")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)
            .AddEndpointFilter<ValidationFilter<UpdateSessionStatusRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        Guid sessionId,
        UpdateSessionStatusRequest request,
        UpdateSessionStatusHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(sessionId, request, cancellationToken);
        return result.ToHttpResult(Results.Ok);
    }
}

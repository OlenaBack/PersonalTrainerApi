using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.WorkoutPlans.GetWorkoutPlans;

internal static class GetWorkoutPlansEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleAsync)
            .WithName("GetWorkoutPlans")
            .WithTags("WorkoutPlans")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly);
    }

    private static async Task<IResult> HandleAsync(
        Guid clientId,
        GetWorkoutPlansHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(clientId, cancellationToken);
        return result.ToHttpResult(Results.Ok);
    }
}

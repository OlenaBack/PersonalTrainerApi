using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Exercises.GetExercises;

internal static class GetExercisesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleAsync)
            .WithName("GetExercises")
            .WithTags("Exercises")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly);
    }

    private static async Task<IResult> HandleAsync(
        GetExercisesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);
        return result.ToHttpResult(Results.Ok);
    }
}

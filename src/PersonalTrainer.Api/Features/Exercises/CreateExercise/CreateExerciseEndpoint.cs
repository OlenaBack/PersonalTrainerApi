using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Exercises.CreateExercise;

internal static class CreateExerciseEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync)
            .WithName("CreateExercise")
            .WithTags("Exercises")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)
            .AddEndpointFilter<ValidationFilter<CreateExerciseRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        CreateExerciseRequest request,
        CreateExerciseHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(response => Results.Created($"/api/exercises/{response.Id}", response));
    }
}

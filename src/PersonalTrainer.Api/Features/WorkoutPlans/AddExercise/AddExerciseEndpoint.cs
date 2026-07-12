using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;

internal static class AddExerciseEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/exercises", HandleAsync)
            .WithName("AddExercise")
            .WithTags("WorkoutPlans")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)
            .AddEndpointFilter<ValidationFilter<AddExerciseRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        Guid planId,
        AddExerciseRequest request,
        AddExerciseHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(planId, request, cancellationToken);
        return result.ToHttpResult(response => Results.Created($"/api/workout-plans/{planId}/exercises/{response.Id}", response));
    }
}

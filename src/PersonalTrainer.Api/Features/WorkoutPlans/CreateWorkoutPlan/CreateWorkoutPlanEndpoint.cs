using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.WorkoutPlans.CreateWorkoutPlan;

internal static class CreateWorkoutPlanEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync)
            .WithName("CreateWorkoutPlan")
            .WithTags("WorkoutPlans")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)
            .AddEndpointFilter<ValidationFilter<CreateWorkoutPlanRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        Guid clientId,
        CreateWorkoutPlanRequest request,
        CreateWorkoutPlanHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(clientId, request, cancellationToken);
        return result.ToHttpResult(response => Results.Created($"/api/workout-plans/{response.Id}", response));
    }
}

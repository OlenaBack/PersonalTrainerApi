using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Sessions.ScheduleSession;

internal static class ScheduleSessionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync)
            .WithName("ScheduleSession")
            .WithTags("Sessions")
            .RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)
            .AddEndpointFilter<ValidationFilter<ScheduleSessionRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        ScheduleSessionRequest request,
        ScheduleSessionHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(response => Results.Created($"/api/sessions/{response.Id}", response));
    }
}

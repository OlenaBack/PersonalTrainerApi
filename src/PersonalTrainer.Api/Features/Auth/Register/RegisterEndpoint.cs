using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Auth.Register;

internal static class RegisterEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/register", HandleAsync)
            .WithName("Register")
            .WithTags("Auth")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        RegisterRequest request,
        RegisterHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(response => Results.Created($"/api/auth/users/{response.UserId}", response));
    }
}

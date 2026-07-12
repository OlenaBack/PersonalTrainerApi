using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Features.Auth.Login;

internal static class LoginEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", HandleAsync)
            .WithName("Login")
            .WithTags("Auth")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<LoginRequest>>();
    }

    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(Results.Ok);
    }
}

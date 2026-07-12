using FluentValidation;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Features.Auth.Login;
using PersonalTrainer.Api.Features.Auth.Register;

namespace PersonalTrainer.Api.Features.Auth;

public static class AuthFeatureExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddScoped<RegisterHandler>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterValidator>();

        services.AddScoped<LoginHandler>();
        services.AddScoped<IValidator<LoginRequest>, LoginValidator>();

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").RequireRateLimiting(RateLimiterPolicyNames.Auth);

        RegisterEndpoint.Map(group);
        LoginEndpoint.Map(group);

        return app;
    }
}

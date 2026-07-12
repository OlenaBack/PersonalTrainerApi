using FluentValidation;
using PersonalTrainer.Api.Features.Sessions.GetSessions;
using PersonalTrainer.Api.Features.Sessions.ScheduleSession;
using PersonalTrainer.Api.Features.Sessions.UpdateSessionStatus;

namespace PersonalTrainer.Api.Features.Sessions;

public static class SessionsFeatureExtensions
{
    public static IServiceCollection AddSessionsFeature(this IServiceCollection services)
    {
        services.AddScoped<ScheduleSessionHandler>();
        services.AddScoped<IValidator<ScheduleSessionRequest>, ScheduleSessionValidator>();

        services.AddScoped<GetSessionsHandler>();

        services.AddScoped<UpdateSessionStatusHandler>();
        services.AddScoped<IValidator<UpdateSessionStatusRequest>, UpdateSessionStatusValidator>();

        return services;
    }

    public static IEndpointRouteBuilder MapSessionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions");

        ScheduleSessionEndpoint.Map(group);
        GetSessionsEndpoint.Map(group);
        UpdateSessionStatusEndpoint.Map(group);

        return app;
    }
}

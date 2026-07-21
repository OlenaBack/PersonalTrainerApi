using PersonalTrainer.Api.Features.Exercises.GetExercises;

namespace PersonalTrainer.Api.Features.Exercises;

public static class ExercisesFeatureExtensions
{
    public static IServiceCollection AddExercisesFeature(this IServiceCollection services)
    {
        services.AddScoped<GetExercisesHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapExercisesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exercises");

        GetExercisesEndpoint.Map(group);

        return app;
    }
}

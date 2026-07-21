using FluentValidation;
using PersonalTrainer.Api.Features.Exercises.CreateExercise;
using PersonalTrainer.Api.Features.Exercises.GetExercises;

namespace PersonalTrainer.Api.Features.Exercises;

public static class ExercisesFeatureExtensions
{
    public static IServiceCollection AddExercisesFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateExerciseHandler>();
        services.AddScoped<IValidator<CreateExerciseRequest>, CreateExerciseValidator>();

        services.AddScoped<GetExercisesHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapExercisesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exercises");

        CreateExerciseEndpoint.Map(group);
        GetExercisesEndpoint.Map(group);

        return app;
    }
}

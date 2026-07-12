using FluentValidation;
using PersonalTrainer.Api.Features.WorkoutPlans.AddExercise;
using PersonalTrainer.Api.Features.WorkoutPlans.CreateWorkoutPlan;
using PersonalTrainer.Api.Features.WorkoutPlans.GetWorkoutPlans;

namespace PersonalTrainer.Api.Features.WorkoutPlans;

public static class WorkoutPlansFeatureExtensions
{
    public static IServiceCollection AddWorkoutPlansFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateWorkoutPlanHandler>();
        services.AddScoped<IValidator<CreateWorkoutPlanRequest>, CreateWorkoutPlanValidator>();

        services.AddScoped<GetWorkoutPlansHandler>();

        services.AddScoped<AddExerciseHandler>();
        services.AddScoped<IValidator<AddExerciseRequest>, AddExerciseValidator>();

        return services;
    }

    public static IEndpointRouteBuilder MapWorkoutPlansEndpoints(this IEndpointRouteBuilder app)
    {
        var clientPlansGroup = app.MapGroup("/api/clients/{clientId:guid}/workout-plans");
        CreateWorkoutPlanEndpoint.Map(clientPlansGroup);
        GetWorkoutPlansEndpoint.Map(clientPlansGroup);

        var planGroup = app.MapGroup("/api/workout-plans/{planId:guid}");
        AddExerciseEndpoint.Map(planGroup);

        return app;
    }
}

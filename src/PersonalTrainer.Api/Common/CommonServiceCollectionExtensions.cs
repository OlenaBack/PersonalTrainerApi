namespace PersonalTrainer.Api.Common;

public static class CommonServiceCollectionExtensions
{
    public static IServiceCollection AddAppCommon(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTrainerAccessor, CurrentTrainerAccessor>();
        services.AddScoped<ICurrentClientAccessor, CurrentClientAccessor>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

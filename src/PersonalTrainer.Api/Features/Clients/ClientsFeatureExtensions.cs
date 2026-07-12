using FluentValidation;
using PersonalTrainer.Api.Features.Clients.CreateClient;
using PersonalTrainer.Api.Features.Clients.DeleteClient;
using PersonalTrainer.Api.Features.Clients.GetClientById;
using PersonalTrainer.Api.Features.Clients.GetClients;
using PersonalTrainer.Api.Features.Clients.UpdateClient;

namespace PersonalTrainer.Api.Features.Clients;

public static class ClientsFeatureExtensions
{
    public static IServiceCollection AddClientsFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateClientHandler>();
        services.AddScoped<IValidator<CreateClientRequest>, CreateClientValidator>();

        services.AddScoped<GetClientsHandler>();
        services.AddScoped<GetClientByIdHandler>();

        services.AddScoped<UpdateClientHandler>();
        services.AddScoped<IValidator<UpdateClientRequest>, UpdateClientValidator>();

        services.AddScoped<DeleteClientHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapClientsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients");

        CreateClientEndpoint.Map(group);
        GetClientsEndpoint.Map(group);
        GetClientByIdEndpoint.Map(group);
        UpdateClientEndpoint.Map(group);
        DeleteClientEndpoint.Map(group);

        return app;
    }
}

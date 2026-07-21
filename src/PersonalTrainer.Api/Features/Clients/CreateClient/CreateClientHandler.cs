using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.Clients.CreateClient;

public sealed class CreateClientHandler(
    IUserProvisioningService provisioningService,
    ICurrentTrainerAccessor currentTrainerAccessor,
    AppDbContext dbContext)
{
    public async Task<Result<ClientResponse>> HandleAsync(CreateClientRequest request, CancellationToken cancellationToken)
    {
        var trainerResult = await currentTrainerAccessor.GetCurrentTrainerOrNotFoundAsync(
            "Clients",
            cancellationToken);
        if (trainerResult.IsFailure)
        {
            return trainerResult.Error!;
        }

        var trainer = trainerResult.Value;

        var provisionResult = await provisioningService.CreateClientAsync(
            request.Email, request.Password, request.FirstName, request.LastName,
            trainer.Id, request.DateOfBirth, cancellationToken);

        if (provisionResult.IsFailure)
        {
            return Result<ClientResponse>.Failure(provisionResult.Error!);
        }

        var client = await dbContext.Clients.SingleAsync(c => c.UserId == provisionResult.Value.Id, cancellationToken);

        return new ClientResponse(client.Id, client.FirstName, client.LastName, client.DateOfBirth, client.CreatedAtUtc);
    }
}

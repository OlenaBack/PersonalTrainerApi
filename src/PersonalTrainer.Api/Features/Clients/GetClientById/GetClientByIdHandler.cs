using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.Clients.GetClientById;

public sealed class GetClientByIdHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<ClientResponse>> HandleAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound("Clients.TrainerProfileNotFound", "No trainer profile found for the current user.");
        }

        var client = await dbContext.Clients
            .SingleOrDefaultAsync(c => c.Id == clientId && c.TrainerId == trainer.Id, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Clients.NotFound", "Client not found.");
        }

        return new ClientResponse(client.Id, client.FirstName, client.LastName, client.DateOfBirth, client.CreatedAtUtc);
    }
}

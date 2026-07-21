using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.Clients.UpdateClient;

public sealed class UpdateClientHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<ClientResponse>> HandleAsync(Guid clientId, UpdateClientRequest request, CancellationToken cancellationToken)
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

        client.FirstName = request.FirstName;
        client.LastName = request.LastName;
        client.DateOfBirth = request.DateOfBirth;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ClientResponse(client.Id, client.FirstName, client.LastName, client.DateOfBirth, client.CreatedAtUtc);
    }
}

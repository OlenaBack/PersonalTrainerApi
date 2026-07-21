using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.Clients.DeleteClient;

public sealed class DeleteClientHandler(
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result> HandleAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var trainerResult = await currentTrainerAccessor.GetCurrentTrainerOrErrorAsync("Clients", cancellationToken);
        if (trainerResult.IsFailure)
        {
            return trainerResult.Error!;
        }

        var trainer = trainerResult.Value;

        var client = await dbContext.Clients
            .SingleOrDefaultAsync(c => c.Id == clientId && c.TrainerId == trainer.Id, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Clients.NotFound", "Client not found.");
        }

        var user = await userManager.FindByIdAsync(client.UserId.ToString());

        dbContext.Clients.Remove(client);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (user is not null)
        {
            await userManager.DeleteAsync(user);
        }

        return Result.Success();
    }
}

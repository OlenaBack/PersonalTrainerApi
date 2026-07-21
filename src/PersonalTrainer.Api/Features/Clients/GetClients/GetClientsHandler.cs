using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;

namespace PersonalTrainer.Api.Features.Clients.GetClients;

public sealed class GetClientsHandler(AppDbContext dbContext, ICurrentTrainerAccessor currentTrainerAccessor)
{
    public async Task<Result<IReadOnlyList<ClientResponse>>> HandleAsync(GetClientsRequest request, CancellationToken cancellationToken)
    {
        var trainerResult = await currentTrainerAccessor.GetCurrentTrainerOrNotFoundAsync(
            "Clients",
            cancellationToken);
        if (trainerResult.IsFailure)
        {
            return trainerResult.Error!;
        }

        var trainer = trainerResult.Value;

        var query = dbContext.Clients.Where(c => c.TrainerId == trainer.Id);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(searchLower) ||
                c.LastName.ToLower().Contains(searchLower));
        }

        if (request.BornAfter is not null)
        {
            query = query.Where(c => c.DateOfBirth > request.BornAfter);
        }

        var clients = await query
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => new ClientResponse(c.Id, c.FirstName, c.LastName, c.DateOfBirth, c.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return clients;
    }
}

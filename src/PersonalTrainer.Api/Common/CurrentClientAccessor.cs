using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Common;

public sealed class CurrentClientAccessor(AppDbContext dbContext, ICurrentUserService currentUserService) : ICurrentClientAccessor
{
    public Task<Client?> GetCurrentClientAsync(CancellationToken cancellationToken)
        => dbContext.Clients.SingleOrDefaultAsync(c => c.UserId == currentUserService.UserId, cancellationToken);
}

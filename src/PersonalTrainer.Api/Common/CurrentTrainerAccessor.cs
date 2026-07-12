using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Common;

public sealed class CurrentTrainerAccessor(AppDbContext dbContext, ICurrentUserService currentUserService) : ICurrentTrainerAccessor
{
    public Task<Trainer?> GetCurrentTrainerAsync(CancellationToken cancellationToken)
        => dbContext.Trainers.SingleOrDefaultAsync(t => t.UserId == currentUserService.UserId, cancellationToken);
}

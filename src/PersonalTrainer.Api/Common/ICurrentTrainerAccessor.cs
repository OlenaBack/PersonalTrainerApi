using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Common;

public interface ICurrentTrainerAccessor
{
    Task<Trainer?> GetCurrentTrainerAsync(CancellationToken cancellationToken);
}

using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Common;

public interface ICurrentClientAccessor
{
    Task<Client?> GetCurrentClientAsync(CancellationToken cancellationToken);
}

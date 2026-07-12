using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Tests.Unit.TestSupport;

public sealed class FakeCurrentTrainerAccessor(Trainer? trainer) : ICurrentTrainerAccessor
{
    public Task<Trainer?> GetCurrentTrainerAsync(CancellationToken cancellationToken) => Task.FromResult(trainer);
}

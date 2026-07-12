using PersonalTrainer.Api.Common;

namespace PersonalTrainer.Api.Tests.Unit.TestSupport;

public sealed class FakeDateTimeProvider(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}

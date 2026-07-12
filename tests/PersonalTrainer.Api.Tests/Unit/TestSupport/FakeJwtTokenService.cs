using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Tests.Unit.TestSupport;

public sealed class FakeJwtTokenService : IJwtTokenService
{
    public string GenerateToken(ApplicationUser user, string role) => $"fake-token:{user.Id}:{role}";
}

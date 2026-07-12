using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Auth;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, string role);
}

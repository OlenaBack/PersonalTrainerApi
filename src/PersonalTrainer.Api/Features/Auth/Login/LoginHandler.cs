using Microsoft.AspNetCore.Identity;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.Auth.Login;

public sealed class LoginHandler(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (role is null)
        {
            return Error.Unauthorized("Auth.NoRoleAssigned", "This account has no assigned role.");
        }

        var accessToken = jwtTokenService.GenerateToken(user, role);

        return new LoginResponse(user.Id, role, accessToken);
    }
}

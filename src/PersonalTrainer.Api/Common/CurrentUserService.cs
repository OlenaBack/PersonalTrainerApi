using System.Security.Claims;

namespace PersonalTrainer.Api.Common;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(value, out var userId))
            {
                throw new InvalidOperationException("No authenticated user in the current context.");
            }

            return userId;
        }
    }

    public string Role
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return value ?? throw new InvalidOperationException("No role claim present for the current user.");
        }
    }
}

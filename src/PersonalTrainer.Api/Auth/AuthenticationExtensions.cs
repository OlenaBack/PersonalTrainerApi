using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PersonalTrainer.Api.Auth;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        // Configured via the named-options + DI pipeline (not a closed-over value read
        // directly off IConfiguration here) so this picks up config added after this
        // method runs - e.g. WebApplicationFactory's test-time overrides - instead of
        // whatever IConfiguration held at service-registration time.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicyNames.TrainerOnly, policy => policy.RequireRole(RoleNames.Trainer))
            .AddPolicy(AuthorizationPolicyNames.ClientOnly, policy => policy.RequireRole(RoleNames.Client));

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}

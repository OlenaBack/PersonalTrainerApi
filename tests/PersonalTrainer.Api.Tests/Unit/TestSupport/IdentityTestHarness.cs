using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Tests.Unit.TestSupport;

public sealed class IdentityTestHarness : IDisposable
{
    private readonly ServiceProvider _rootProvider;
    private readonly IServiceScope _scope;

    private IdentityTestHarness(ServiceProvider rootProvider, IServiceScope scope)
    {
        _rootProvider = rootProvider;
        _scope = scope;
    }

    public AppDbContext DbContext => _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    public UserManager<ApplicationUser> UserManager => _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    public static async Task<IdentityTestHarness> CreateAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        var rootProvider = services.BuildServiceProvider();
        var scope = rootProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var roleName in RoleNames.All)
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }

        return new IdentityTestHarness(rootProvider, scope);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _rootProvider.Dispose();
    }
}

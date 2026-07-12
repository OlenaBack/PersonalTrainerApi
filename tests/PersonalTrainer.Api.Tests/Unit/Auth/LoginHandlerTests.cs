using FluentAssertions;
using PersonalTrainer.Api.Auth;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Domain;
using PersonalTrainer.Api.Features.Auth.Login;
using PersonalTrainer.Api.Tests.Unit.TestSupport;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Auth;

public class LoginHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsToken()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var user = new ApplicationUser { UserName = "trainer@example.com", Email = "trainer@example.com" };
        await harness.UserManager.CreateAsync(user, "Password1");
        await harness.UserManager.AddToRoleAsync(user, RoleNames.Trainer);

        var handler = new LoginHandler(harness.UserManager, new FakeJwtTokenService());

        var result = await handler.HandleAsync(new LoginRequest("trainer@example.com", "Password1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(RoleNames.Trainer);
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ReturnsUnauthorized()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var user = new ApplicationUser { UserName = "trainer@example.com", Email = "trainer@example.com" };
        await harness.UserManager.CreateAsync(user, "Password1");
        await harness.UserManager.AddToRoleAsync(user, RoleNames.Trainer);

        var handler = new LoginHandler(harness.UserManager, new FakeJwtTokenService());

        var result = await handler.HandleAsync(new LoginRequest("trainer@example.com", "WrongPassword"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task HandleAsync_UnknownEmail_ReturnsUnauthorized()
    {
        using var harness = await IdentityTestHarness.CreateAsync();
        var handler = new LoginHandler(harness.UserManager, new FakeJwtTokenService());

        var result = await handler.HandleAsync(new LoginRequest("nobody@example.com", "Password1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

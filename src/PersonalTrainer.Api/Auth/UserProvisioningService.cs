using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalTrainer.Api.Common;
using PersonalTrainer.Api.Data;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Auth;

public sealed class UserProvisioningService(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : IUserProvisioningService
{
    public async Task<Result<ApplicationUser>> CreateTrainerAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Error.Conflict("Users.EmailAlreadyRegistered", "An account with this email already exists.");
        }

        var user = new ApplicationUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return Error.Validation("Users.CreationFailed", DescribeErrors(createResult));
        }

        await userManager.AddToRoleAsync(user, RoleNames.Trainer);

        dbContext.Trainers.Add(new Trainer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            CreatedAtUtc = dateTimeProvider.UtcNow,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<Result<ApplicationUser>> CreateClientAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        Guid trainerId,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Error.Conflict("Users.EmailAlreadyRegistered", "An account with this email already exists.");
        }

        var trainerExists = await dbContext.Trainers.AnyAsync(t => t.Id == trainerId, cancellationToken);
        if (!trainerExists)
        {
            return Error.NotFound("Users.TrainerNotFound", "The specified trainer does not exist.");
        }

        var user = new ApplicationUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return Error.Validation("Users.CreationFailed", DescribeErrors(createResult));
        }

        await userManager.AddToRoleAsync(user, RoleNames.Client);

        dbContext.Clients.Add(new Client
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TrainerId = trainerId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            CreatedAtUtc = dateTimeProvider.UtcNow,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    private static string DescribeErrors(IdentityResult result)
        => string.Join(" ", result.Errors.Select(e => e.Description));
}

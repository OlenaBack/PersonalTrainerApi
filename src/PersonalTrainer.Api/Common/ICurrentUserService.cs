namespace PersonalTrainer.Api.Common;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }
}

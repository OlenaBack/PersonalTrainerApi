using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.Sessions.UpdateSessionStatus;

public sealed record UpdateSessionStatusRequest(SessionStatus Status);

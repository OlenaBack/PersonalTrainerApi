using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Features.Sessions.GetSessions;

public sealed record GetSessionsRequest(DateTime? From, DateTime? To, SessionStatus? Status);

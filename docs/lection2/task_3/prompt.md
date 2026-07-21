In src/PersonalTrainer.Api/Features/Clients/, the handlers (CreateClient, UpdateClient, DeleteClient, GetClientById, GetClients) each repeat a check that gets the current trainer and returns NotFound if none exists, with a feature-specific error code.

Extract this into a reusable helper on ICurrentTrainerAccessor in Common/, following the existing Result<T> pattern (see Common/Result.cs) — use its implicit Error conversion rather than explicit Result<T>.Failure(...) calls.

Keep each handler's existing error code exactly as-is. Don't change any other logic. Show me the diff before applying.
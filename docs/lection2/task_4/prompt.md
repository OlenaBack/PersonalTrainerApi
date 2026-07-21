[SCOPE]
In src/PersonalTrainer.Api/Features/Clients/, the handlers CreateClient,
UpdateClient, DeleteClient, GetClientById, and GetClients each repeat the
same check: fetch the current trainer via ICurrentTrainerAccessor and
return a NotFound error if no trainer profile exists, using a
feature-specific error code (e.g. "Clients.TrainerProfileNotFound").
[FOLLOW EXISTING CONVENTIONS]
Extract this into a reusable extension method on ICurrentTrainerAccessor
in Common/, following the existing Result<T> pattern (see Common/Result.cs)
— use its implicit Error conversion rather than explicit
Result<T>.Failure(...) calls. Don't introduce a new return-value pattern
(no callbacks, no monadic Bind, no generic type parameters for
feature identification) — keep the same linear, single-HandleAsync
handler shape already used throughout this codebase.
[SUCCESS CRITERION]
The helper must fully eliminate duplication: no error message text and
no repeated substring of the error code should remain at call sites.
Take a short "feature prefix" string (e.g. "Clients") as the parameter,
and build the full error code internally using a private const for the
fixed "TrainerProfileNotFound" suffix.
[BOUNDARIES]
Keep each handler's existing final error code exactly as-is
(e.g. "Clients.TrainerProfileNotFound", "Clients.NotFound" for the
separate missing-client case). Don't change any other logic: EF Core
queries, filters, provisioning calls, deletion logic, field updates, or
response mapping must stay identical to the current code.
[APPROVAL GATE]https://github.com/OlenaBack/PersonalTrainerApi/tree/master
Show me the diff before applying.
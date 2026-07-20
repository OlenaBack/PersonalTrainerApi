# AddExercise Slice Review — `WorkoutPlans`

**Files reviewed:**
- [src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseRequest.cs](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseRequest.cs)
- [src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs)
- [src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseValidator.cs](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseValidator.cs)
- [src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs)

Supporting files consulted: [ExerciseResponse.cs](../src/PersonalTrainer.Api/Features/WorkoutPlans/ExerciseResponse.cs), [WorkoutPlansFeatureExtensions.cs](../src/PersonalTrainer.Api/Features/WorkoutPlans/WorkoutPlansFeatureExtensions.cs), [Common/Result.cs](../src/PersonalTrainer.Api/Common/Result.cs), [Common/ResultHttpExtensions.cs](../src/PersonalTrainer.Api/Common/ResultHttpExtensions.cs), [Common/Error.cs](../src/PersonalTrainer.Api/Common/Error.cs), [Common/ValidationFilter.cs](../src/PersonalTrainer.Api/Common/ValidationFilter.cs), [Common/ICurrentTrainerAccessor.cs](../src/PersonalTrainer.Api/Common/ICurrentTrainerAccessor.cs) / [CurrentTrainerAccessor.cs](../src/PersonalTrainer.Api/Common/CurrentTrainerAccessor.cs), [Domain/Exercise.cs](../src/PersonalTrainer.Api/Domain/Exercise.cs), [Auth/AuthorizationPolicyNames.cs](../src/PersonalTrainer.Api/Auth/AuthorizationPolicyNames.cs).

---

## SUMMARY

The AddExercise slice lets an authenticated trainer append a single exercise (name, sets, reps, weight, notes, ordering, tags) to one of their own workout plans. It is a `POST /api/workout-plans/{planId}/exercises` endpoint that validates input, confirms the calling trainer owns the target plan, persists a new `Exercise` row, and returns the created resource as a `201 Created`. The slice is small and self-contained — four files, one EF Core write, one ownership check.

---

## 1. Responsibility & business requirement

The user-facing need: a trainer building out a client's workout plan needs to add individual exercises to it (e.g., "Squat, 3 sets, 10 reps, 60kg") one at a time, in a defined order, with optional notes and tags for categorization (e.g. "Legs", "Core").

This is implemented as: `AddExerciseHandler.HandleAsync` ([AddExerciseHandler.cs:10-41](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L10-L41)) creates a new `Exercise` domain entity scoped to a `WorkoutPlanId` (the `planId` route parameter) and saves it via EF Core. The response (`ExerciseResponse`, [ExerciseResponse.cs:3-12](../src/PersonalTrainer.Api/Features/WorkoutPlans/ExerciseResponse.cs#L3-L12)) echoes back the persisted exercise including its server-generated `Id`.

Business rule embedded here: an exercise can only be added to a plan that belongs to a client of the *currently authenticated trainer* — see Section 4.

## 2. Architecture pattern

**General convention in this repo:** each "slice" (one API operation) lives in its own folder under `Features/<AreaName>/<SliceName>/` and is composed of up to four files:
- `<Slice>Request.cs` — an immutable `record` defining the input DTO.
- `<Slice>Handler.cs` — the class containing the actual business logic, injected via DI, returning `Result` or `Result<T>`.
- `<Slice>Validator.cs` — a `FluentValidation.AbstractValidator<TRequest>` enforcing input-shape rules.
- `<Slice>Endpoint.cs` — a static class with a `Map(IEndpointRouteBuilder)` method that wires the HTTP route, auth policy, and validation filter, and adapts the `Result` to an `IResult` HTTP response.

This is confirmed across sibling slices (`CreateWorkoutPlan`, `GetWorkoutPlans`, `ScheduleSession`, `CreateClient`, etc. — [Glob results], all following the same four-file shape), and area-level registration is centralized in a `<Area>FeatureExtensions.cs` (e.g. [WorkoutPlansFeatureExtensions.cs](../src/PersonalTrainer.Api/Features/WorkoutPlans/WorkoutPlansFeatureExtensions.cs)) that registers handlers/validators in DI and maps routes onto an `IEndpointRouteBuilder` group.

**How AddExercise follows it, specifically:**
- `AddExerciseRequest` ([AddExerciseRequest.cs:3-10](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseRequest.cs#L3-L10)) — a `sealed record` with `Name`, `Sets`, `Reps`, `WeightKg`, `Notes`, `OrderIndex`, `Tags`.
- `AddExerciseHandler` ([AddExerciseHandler.cs:8](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L8)) — primary-constructor class taking `AppDbContext` and `ICurrentTrainerAccessor`, registered as `AddScoped<AddExerciseHandler>()` in [WorkoutPlansFeatureExtensions.cs:17](../src/PersonalTrainer.Api/Features/WorkoutPlans/WorkoutPlansFeatureExtensions.cs#L17).
- `AddExerciseValidator` ([AddExerciseValidator.cs:5-17](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseValidator.cs#L5-L17)) — registered as `AddScoped<IValidator<AddExerciseRequest>, AddExerciseValidator>()` in [WorkoutPlansFeatureExtensions.cs:18](../src/PersonalTrainer.Api/Features/WorkoutPlans/WorkoutPlansFeatureExtensions.cs#L18).
- `AddExerciseEndpoint` ([AddExerciseEndpoint.cs:6-26](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L6-L26)) — maps `POST /exercises` onto the `/api/workout-plans/{planId:guid}` route group defined in [WorkoutPlansFeatureExtensions.cs:29-30](../src/PersonalTrainer.Api/Features/WorkoutPlans/WorkoutPlansFeatureExtensions.cs#L29-L30), giving the full route `POST /api/workout-plans/{planId}/exercises`.

One deviation worth flagging: `HandleAsync` in the endpoint builds the `Location` header using a hardcoded literal `$"/api/workout-plans/{planId}/exercises/{response.Id}"` ([AddExerciseEndpoint.cs:24](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L24)) rather than deriving it from the route group, whereas the actual mapped path is built from `planGroup` + `/exercises`. They currently agree, but nothing enforces they stay in sync if the group prefix changes.

**What `Result<T>` represents:** it's a discriminated-union-style outcome wrapper defined in [Common/Result.cs](../src/PersonalTrainer.Api/Common/Result.cs). `Result<TValue>` ([Result.cs:31-54](../src/PersonalTrainer.Api/Common/Result.cs#L31-L54)) is either a success carrying a `TValue` payload or a failure carrying an `Error` (code, message, `ErrorType`) — never both (enforced by the base `Result` constructor at [Result.cs:5-19](../src/PersonalTrainer.Api/Common/Result.cs#L5-L19)). It has implicit conversion operators from both `TValue` and `Error`, which is why `AddExerciseHandler.HandleAsync` can `return Error.NotFound(...)` ([AddExerciseHandler.cs:15](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L15), [:21](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L21)) or `return new ExerciseResponse(...)` ([AddExerciseHandler.cs:40](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L40)) directly without explicit wrapping. The endpoint then converts it to HTTP via `result.ToHttpResult(...)` ([ResultHttpExtensions.cs:5-6](../src/PersonalTrainer.Api/Common/ResultHttpExtensions.cs#L5-L6)), which maps `ErrorType` to a status code (`NotFound` → 404, etc. — [ResultHttpExtensions.cs:13-21](../src/PersonalTrainer.Api/Common/ResultHttpExtensions.cs#L13-L21)) — this is how the handler stays free of any HTTP/`IResult` concerns, keeping business logic testable in isolation (see Section 5).

## 3. External dependencies

- **`AppDbContext`** (EF Core `DbContext`, from `PersonalTrainer.Api.Data`) — injected into `AddExerciseHandler` ([AddExerciseHandler.cs:8](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L8)). Used to: query `dbContext.WorkoutPlans` for plan ownership ([AddExerciseHandler.cs:18](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L18)), insert into `dbContext.Exercises` ([AddExerciseHandler.cs:37](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L37)), and commit via `SaveChangesAsync` ([AddExerciseHandler.cs:38](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L38)).
- **`ICurrentTrainerAccessor`** — injected into the handler ([AddExerciseHandler.cs:8](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L8)), used at [AddExerciseHandler.cs:12](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L12) to resolve the `Trainer` row for the currently authenticated user. Its concrete implementation, `CurrentTrainerAccessor` ([CurrentTrainerAccessor.cs:7-11](../src/PersonalTrainer.Api/Common/CurrentTrainerAccessor.cs#L7-L11)), itself depends on `AppDbContext` and `ICurrentUserService` (looks up `Trainers` by `UserId == currentUserService.UserId`) — so identity resolution is a further indirection on top of the auth token.
- **`IValidator<AddExerciseRequest>`** (FluentValidation) — not injected into the handler directly; it's resolved inside `ValidationFilter<TRequest>` ([ValidationFilter.cs:15](../src/PersonalTrainer.Api/Common/ValidationFilter.cs#L15)) as an ASP.NET Core endpoint filter, applied via `.AddEndpointFilter<ValidationFilter<AddExerciseRequest>>()` ([AddExerciseEndpoint.cs:14](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L14)), so validation runs *before* the handler is invoked at all.
- **ASP.NET Core authorization middleware** — `.RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)` ([AddExerciseEndpoint.cs:13](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L13)) is a framework-level dependency, not DI-injected into the slice's own code.

## 4. Security considerations

**Authorization / role check:** the route requires the `TrainerOnly` policy ([AddExerciseEndpoint.cs:13](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L13)), which maps to `RequireRole(RoleNames.Trainer)` ([Auth/AuthenticationExtensions.cs:37](../src/PersonalTrainer.Api/Auth/AuthenticationExtensions.cs#L37)). This only proves the caller is *a* trainer — not that they own the target plan.

**Ownership check (the actual authorization boundary for this slice):** `AddExerciseHandler.HandleAsync` performs two checks before writing anything:
1. Resolves the trainer profile for the current user; if none exists, returns `Error.NotFound("WorkoutPlans.TrainerProfileNotFound", ...)` ([AddExerciseHandler.cs:12-16](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L12-L16)).
2. Queries `dbContext.WorkoutPlans.AnyAsync(w => w.Id == planId && w.TrainerId == trainer.Id, ...)` ([AddExerciseHandler.cs:18](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L18)) — if the plan doesn't exist *or* belongs to a different trainer, it returns `Error.NotFound("WorkoutPlans.NotFound", ...)` ([AddExerciseHandler.cs:19-22](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L19-L22)).

Note this deliberately returns `404 Not Found` rather than `403 Forbidden` for a plan owned by someone else — this avoids confirming to a caller that a given `planId` exists at all, which is a reasonable choice to prevent resource enumeration, and it's explicitly covered by a test (see Section 5).

**Input validation:** `AddExerciseValidator` enforces: `Name` non-empty, max 200 chars; `Sets` > 0; `Reps` > 0; `WeightKg` ≥ 0 when present; `Notes` max 1000 chars; `OrderIndex` ≥ 0; each `Tags` entry non-empty and max 50 chars ([AddExerciseValidator.cs:9-15](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseValidator.cs#L9-L15)). This runs in `ValidationFilter<TRequest>` before the handler executes, and failures short-circuit into a `400` `ValidationProblem` ([ValidationFilter.cs:21-29](../src/PersonalTrainer.Api/Common/ValidationFilter.cs#L21-L29)) without ever reaching `AddExerciseHandler`.

**Input trusted without verification:**
- `Tags` has no upper bound on the *number* of tags — only per-tag length is capped. A client could submit an arbitrarily long `Tags` array; there is no `RuleFor(x => x.Tags).Must(t => t.Count <= N)` guard.
- `OrderIndex` is accepted as caller-supplied and stored as-is ([AddExerciseHandler.cs:33](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L33)) with no check against existing exercises in the plan — duplicate or out-of-range `OrderIndex` values across exercises in the same plan are possible; ordering uniqueness is not enforced at validator, handler, or (as far as these four files show) database-constraint level.
- The `planId` route parameter is the sole authorization key tying the write to a specific plan; there's no separate check that the plan's `ClientId` is valid/active — only that the requesting trainer owns the row.

## 5. Test coverage

**Unit tests:** [tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs](../tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs) contains two `[Fact]` tests against `AddExerciseHandler` directly (using an in-memory `TestDbContextFactory` and a `FakeCurrentTrainerAccessor`):
- `HandleAsync_OwnPlan_AddsExercise` ([AddExerciseHandlerTests.cs:12-32](../tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs#L12-L32)) — happy path: trainer adds an exercise to their own plan, asserts `IsSuccess`, returned `Name`, and `Tags`.
- `HandleAsync_PlanOwnedByOtherTrainer_ReturnsNotFound` ([AddExerciseHandlerTests.cs:34-54](../tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs#L34-L54)) — negative path: plan belongs to a different trainer, asserts `IsFailure` and `ErrorType.NotFound`.

**Integration tests:** [tests/PersonalTrainer.Api.Tests/Integration/WorkoutPlansEndpointsTests.cs](../tests/PersonalTrainer.Api.Tests/Integration/WorkoutPlansEndpointsTests.cs) has one test, `CreatePlan_AddExercise_ThenGetPlans_ReturnsPlan` ([WorkoutPlansEndpointsTests.cs:17-40](../tests/PersonalTrainer.Api.Tests/Integration/WorkoutPlansEndpointsTests.cs#L17-L40)), which drives the full HTTP pipeline (register trainer → create client → create plan → `POST .../exercises` → asserts `201 Created`) via a real `HttpClient` against `CustomWebApplicationFactory`. This is the only slice-level HTTP test and it only checks the happy path status code — it does not deserialize/assert the `ExerciseResponse` body, does not exercise the validation-failure (`400`) path over HTTP, and does not exercise the cross-tenant `404` path over HTTP (that's unit-tested only, against the handler directly).

**Not found / gaps:**
- No dedicated test file for `AddExerciseValidator` (e.g. no `AddExerciseValidatorTests.cs`) — this appears consistent with the rest of the repo, as no `*Validator*Tests*.cs` file exists anywhere under `tests/`, so validators are only indirectly exercised via the integration test's happy-path input.
- No test asserts `TrainerOnly` policy rejection (e.g. a non-trainer role getting `403`) specifically for this endpoint.
- No test covers the "no trainer profile" branch ([AddExerciseHandler.cs:13-16](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L13-L16)).

## 6. Main risks

1. **`OrderIndex` and `Tags` have no collection-level invariants enforced anywhere in this slice.** Nothing prevents two exercises in the same plan from sharing an `OrderIndex`, and nothing caps how many tags a single request can attach. If a future feature (e.g. drag-to-reorder) assumes `OrderIndex` is unique per plan, or a UI assumes a bounded tag list, that assumption isn't backed by validation, handler logic, or (visibly) a DB constraint — a developer extending this slice should not assume uniqueness/bounds exist just because the shape suggests them.
2. **The ownership check is a single `AnyAsync` gate, and everything downstream trusts it.** All plan-scoping safety comes from [AddExerciseHandler.cs:18](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L18) (`w.Id == planId && w.TrainerId == trainer.Id`). If this slice is ever refactored — e.g. to skip the check for a "bulk add" variant, or if `planId` starts being sourced from the body instead of the route — the tenant-isolation guarantee silently disappears with it, since there's no secondary enforcement (e.g. a DB-level filter, a shared authorization service) backing it up. The existing negative unit test ([AddExerciseHandlerTests.cs:34-54](../tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs#L34-L54)) is what currently guards this — treat it as load-bearing and keep it passing.

---

## CLOSING SUMMARY

The single most important thing to know before touching this code: the *only* thing standing between a trainer and writing an exercise into another trainer's workout plan is the one-line ownership check at [AddExerciseHandler.cs:18](../src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L18) (`w.Id == planId && w.TrainerId == trainer.Id`), and it deliberately returns `404` rather than `403` to avoid leaking plan existence. Any change to this slice — adding bulk-insert, changing how `planId` is sourced, or introducing a new endpoint that touches `Exercises` — must preserve that exact check (and its unit-test coverage), since neither `Result<T>`/`ExerciseResponse` nor the `TrainerOnly` role policy provide tenant isolation on their own.

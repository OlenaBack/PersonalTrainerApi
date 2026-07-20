# AddExercise Slice Review (WorkoutPlans)

## SUMMARY

This slice lets an authenticated trainer attach one exercise from their own exercise catalog to a specific workout plan they own, recording the prescription (sets, reps, weight, notes, order). It is implemented as a self-contained vertical slice — one `HandleAsync` call validates the request, re-verifies ownership of both the parent plan and the referenced exercise against the database, then persists a new `WorkoutPlanExercise` line item and returns it as `201 Created`.

---

## 1. Responsibility & business requirement

The user-facing need: a trainer builds up a client's workout plan by picking exercises from their own reusable exercise catalog and specifying how that exercise should be performed within *this* plan (sets, reps, weight, notes, position in the list).

This is implemented by `AddExerciseHandler.HandleAsync` ([AddExerciseHandler.cs:10-58](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L10-L58)), which creates a new `WorkoutPlanExercise` row — the "per-plan line item" — that links a `WorkoutPlan` to a `Exercise` catalog entry, per the domain split described in [WorkoutPlanExercise.cs:1-17](src/PersonalTrainer.Api/Domain/WorkoutPlanExercise.cs#L1-L17) and [Exercise.cs:1-12](src/PersonalTrainer.Api/Domain/Exercise.cs#L1-L12) (this split was introduced in commit `743e4cd`, "Split Exercise into a trainer-owned catalog and per-plan line item").

- `Exercise` ([Exercise.cs:3-12](src/PersonalTrainer.Api/Domain/Exercise.cs#L3-L12)) is the trainer's reusable catalog: `Id`, `TrainerId`, `Name`, `Tags`, `CreatedAtUtc`.
- `WorkoutPlanExercise` ([WorkoutPlanExercise.cs:3-17](src/PersonalTrainer.Api/Domain/WorkoutPlanExercise.cs#L3-L17)) is the join/line-item entity: it references `WorkoutPlanId` + `ExerciseId`, denormalizes `TrainerId`, and carries the plan-specific prescription fields `Sets`, `Reps`, `WeightKg`, `Notes`, `OrderIndex`.

## 2. Architecture pattern

**General convention in this repo:** each vertical slice lives in its own folder under `Features/<Area>/<UseCase>/` and is composed of up to four files:

- **Request** — an immutable `record` describing the input shape (e.g. [AddExerciseRequest.cs:3-9](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseRequest.cs#L3-L9)).
- **Validator** — a `FluentValidation` `AbstractValidator<TRequest>` with input-shape rules only (e.g. [AddExerciseValidator.cs:5-16](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseValidator.cs#L5-L16)).
- **Handler** — the use-case logic: loads dependencies via constructor DI, performs authorization/ownership checks, touches the DB, and returns a `Result<T>` (e.g. [AddExerciseHandler.cs:8](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L8)).
- **Endpoint** — a static class that maps the route, attaches `RequireAuthorization`/`AddEndpointFilter`, and translates the `Result<T>` into an `IResult` (e.g. [AddExerciseEndpoint.cs:6-26](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L6-L26)).

This same four-file shape is used consistently elsewhere in the repo (`CreateExerciseHandler`, `CreateWorkoutPlanHandler`, `ScheduleSessionHandler`, etc., all under `src/PersonalTrainer.Api/Features/**`).

**How `AddExercise` follows it, specifically:**

- `AddExerciseRequest` ([AddExerciseRequest.cs:3-9](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseRequest.cs#L3-L9)) — `ExerciseId`, `Sets`, `Reps`, `WeightKg?`, `Notes?`, `OrderIndex`. Note `PlanId` is *not* part of this record — it comes from the route, not the body (see §3/§4).
- `AddExerciseValidator` ([AddExerciseValidator.cs:9-14](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseValidator.cs#L9-L14)) enforces shape-only rules: `ExerciseId` not empty, `Sets`/`Reps` > 0, `WeightKg` ≥ 0 when present, `Notes` ≤ 1000 chars, `OrderIndex` ≥ 0.
- `AddExerciseHandler` ([AddExerciseHandler.cs:10-58](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L10-L58)) resolves the current trainer, re-checks ownership of the plan and the exercise, inserts the `WorkoutPlanExercise`, and maps the new entity plus catalog data into a `WorkoutPlanExerciseResponse` ([WorkoutPlanExerciseResponse.cs:3-13](src/PersonalTrainer.Api/Features/WorkoutPlans/WorkoutPlanExerciseResponse.cs#L3-L13)).
- `AddExerciseEndpoint` ([AddExerciseEndpoint.cs:8-15](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L8-L15)) maps `POST /exercises`, requires the `TrainerOnly` authorization policy, and attaches `ValidationFilter<AddExerciseRequest>` so the validator runs before the handler; `HandleAsync` ([AddExerciseEndpoint.cs:17-25](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L17-L25)) binds `planId` from the route and turns a successful `Result<WorkoutPlanExerciseResponse>` into `201 Created`.

**What `Result<T>` represents:** it's the repo's railway-oriented-programming return type for use cases that can fail in an *expected* (non-exceptional) way. Defined in [Result.cs:3-54](src/PersonalTrainer.Api/Common/Result.cs#L3-L54), `Result<TValue>` is a sealed wrapper that is either a success carrying a `TValue` or a failure carrying an `Error` — never both (enforced by the base `Result` constructor at [Result.cs:5-19](src/PersonalTrainer.Api/Common/Result.cs#L5-L19)). Handlers return `Error.NotFound(...)` or a value directly, relying on implicit conversion operators ([Result.cs:52-53](src/PersonalTrainer.Api/Common/Result.cs#L52-L53)). At the HTTP boundary, `ResultHttpExtensions.ToHttpResult` ([ResultHttpExtensions.cs:5-27](src/PersonalTrainer.Api/Common/ResultHttpExtensions.cs#L5-L27)) maps `Error.Type` to a status code (`NotFound` → 404, `Validation` → 400, etc.) and produces a `Results.Problem(...)`, or invokes the success callback otherwise. This keeps domain/authorization failures out of exception-handling paths entirely.

## 3. External dependencies

- **`AppDbContext`** (EF Core) — injected into `AddExerciseHandler` via primary constructor ([AddExerciseHandler.cs:8](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L8)). Used to query `WorkoutPlans`, `Exercises`, and to add to `WorkoutPlanExercises` ([AddExerciseHandler.cs:18](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L18), [:24](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L24), [:44](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L44)).
- **`ICurrentTrainerAccessor`** — DI-injected service ([AddExerciseHandler.cs:8](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L8), interface at [ICurrentTrainerAccessor.cs:5-8](src/PersonalTrainer.Api/Common/ICurrentTrainerAccessor.cs#L5-L8)). Its concrete implementation `CurrentTrainerAccessor` ([CurrentTrainerAccessor.cs:7-11](src/PersonalTrainer.Api/Common/CurrentTrainerAccessor.cs#L7-L11)) looks up the `Trainer` row whose `UserId` matches `ICurrentUserService.UserId` — i.e. it derives the trainer identity from the authenticated principal, not from any client-supplied ID.
- **`IValidator<AddExerciseRequest>`** (FluentValidation) — resolved by the `ValidationFilter<TRequest>` endpoint filter ([ValidationFilter.cs:5-33](src/PersonalTrainer.Api/Common/ValidationFilter.cs#L5-L33)), not by the handler directly.
- **ASP.NET Core authorization** — the `TrainerOnly` policy ([AuthorizationPolicyNames.cs:5](src/PersonalTrainer.Api/Auth/AuthorizationPolicyNames.cs#L5)), which requires the `Trainer` role ([AuthenticationExtensions.cs:43](src/PersonalTrainer.Api/Auth/AuthenticationExtensions.cs#L43)), applied at the endpoint ([AddExerciseEndpoint.cs:13](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L13)).

No other external services (no HTTP clients, no message bus, no caching) are used by this slice.

## 4. Security considerations

**Auth/ownership checks present:**
1. Route-level: `RequireAuthorization(AuthorizationPolicyNames.TrainerOnly)` ([AddExerciseEndpoint.cs:13](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L13)) rejects any caller who isn't in the `Trainer` role before the handler ever runs.
2. Trainer resolution: the handler never trusts a trainer ID from the client — it derives one from the authenticated user via `currentTrainerAccessor.GetCurrentTrainerAsync` ([AddExerciseHandler.cs:12](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L12)) and fails closed with `NotFound` if no trainer profile exists ([AddExerciseHandler.cs:13-16](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L13-L16)).
3. Plan ownership: `dbContext.WorkoutPlans.AnyAsync(w => w.Id == planId && w.TrainerId == trainer.Id, ...)` ([AddExerciseHandler.cs:18](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L18)) — a plan ID belonging to another trainer resolves to `NotFound`, not `Forbidden`, which avoids confirming the plan's existence to a non-owner.
4. Exercise ownership: `dbContext.Exercises.SingleOrDefaultAsync(e => e.Id == request.ExerciseId && e.TrainerId == trainer.Id, ...)` ([AddExerciseHandler.cs:24-25](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L24-L25)) — a trainer cannot attach another trainer's catalog exercise to their own plan.

**Input validation:** `AddExerciseValidator` enforces only shape/range constraints ([AddExerciseValidator.cs:9-14](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseValidator.cs#L9-L14)) — it cannot and does not check ownership (FluentValidation here has no DB access); all trust/ownership decisions are deferred to the handler, which is the correct split of responsibility for this codebase.

**What is trusted without further verification:**
- `planId` is bound directly from the route ([AddExerciseEndpoint.cs:18](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L18)) with no format/GUID validation beyond ASP.NET Core's model binding — but this is safe because the subsequent DB query re-scopes it to `w.TrainerId == trainer.Id` ([AddExerciseHandler.cs:18](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L18)), so an invalid or unauthorized ID simply yields `NotFound`.
- `Sets`, `Reps`, `WeightKg`, `Notes`, `OrderIndex` are persisted essentially as-is once they pass the validator's numeric/length checks ([AddExerciseHandler.cs:37-41](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L37-L41)) — there is no upper bound on `Sets`/`Reps`/`WeightKg`, and no server-side check that `OrderIndex` is unique or contiguous within the plan (confirmed against [WorkoutPlanExerciseConfiguration.cs:32-33](src/PersonalTrainer.Api/Data/Configurations/WorkoutPlanExerciseConfiguration.cs#L32-L33), which only indexes `WorkoutPlanId` and `ExerciseId` individually — no unique/composite index exists). `Notes` is stored as free text with only a length cap; it is not HTML-encoded here, so any XSS risk depends entirely on how a future UI renders it.
- Nothing prevents the same `ExerciseId` from being added to the same plan more than once — there's no uniqueness check in the handler or a DB constraint on `(WorkoutPlanId, ExerciseId)`.

## 5. Test coverage

**Unit tests:** yes — [tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs](tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs) contains three tests against `AddExerciseHandler` directly (using a real `AppDbContext` via `TestDbContextFactory` and a `FakeCurrentTrainerAccessor`):
- `HandleAsync_OwnPlan_AddsExercise` ([:12-35](tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs#L12-L35)) — happy path, asserts the response carries the exercise's `Name`/`Tags` and the requested `Sets`.
- `HandleAsync_PlanOwnedByOtherTrainer_ReturnsNotFound` ([:37-59](tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs#L37-L59)) — verifies the plan-ownership check.
- `HandleAsync_ExerciseOwnedByOtherTrainer_ReturnsNotFound` ([:61-83](tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs#L61-L83)) — verifies the exercise-ownership check.

**Integration tests:** none found. `tests/PersonalTrainer.Api.Tests/Integration/` contains only `ExercisesEndpointsTests.cs`, which exercises the `Exercises` feature's own endpoints, not `WorkoutPlans/AddExercise`. This means the HTTP-level wiring for this slice — the `TrainerOnly` policy, the `ValidationFilter<AddExerciseRequest>` returning `400` on invalid input, and the `201 Created` response including its `Location` header — is **not** covered by any automated test today.

## 6. Main risks

1. **No duplicate/uniqueness protection.** Nothing stops a trainer from adding the same `ExerciseId` to the same plan multiple times, and nothing stops two line items from sharing the same `OrderIndex`. There's no unique index on `WorkoutPlanExercise` beyond the individual `WorkoutPlanId`/`ExerciseId` indexes ([WorkoutPlanExerciseConfiguration.cs:32-33](src/PersonalTrainer.Api/Data/Configurations/WorkoutPlanExerciseConfiguration.cs#L32-L33)) and no handler-side check. If you add a "reorder" or "list exercises in a plan" feature, don't assume `OrderIndex` values are unique or gapless.
2. **No endpoint/integration test exists for this slice.** Because coverage stops at the handler, a regression in the `TrainerOnly` policy wiring, the `ValidationFilter<AddExerciseRequest>` registration, or the route/`planId` binding in `AddExerciseEndpoint.HandleAsync` ([AddExerciseEndpoint.cs:17-25](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs#L17-L25)) would not be caught by the current test suite. Any change to auth policy names, filter registration, or route shape should be paired with a new integration test (following the pattern in `ExercisesEndpointsTests.cs`) rather than relying on the existing unit tests.

---

## CLOSING SUMMARY

The single most important thing to know before touching this code: **all authorization here is ownership-by-requery, not trust-by-ID** — the handler always re-derives the trainer from the authenticated principal and re-checks `TrainerId` on both the plan and the exercise directly against the database ([AddExerciseHandler.cs:12-29](src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs#L12-L29)), rather than trusting any ID supplied by the client. Preserve that pattern in any modification, and be aware that duplicate exercises/order indices within a plan are currently unguarded and untested at the HTTP layer.

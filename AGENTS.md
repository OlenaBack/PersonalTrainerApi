# AGENTS.md — Personal Trainer API

.NET 10 minimal API with vertical slice architecture. No MVC controllers.

EF Core with Npgsql/PostgreSQL, FluentValidation, ASP.NET Core Identity with JWT bearer authentication, and roles `Trainer` and `Client`.

Unit tests use xUnit with EF Core InMemory. Integration tests use Testcontainers.PostgreSql and require Docker.

## Layout

```text
Features/{Area}/{UseCase}/
  {UseCase}Request.cs    // when a request model is needed
  {UseCase}Handler.cs
  {UseCase}Validator.cs  // when request validation is needed
  {UseCase}Endpoint.cs
```

Shared response DTOs live at the feature-area level.

Each area registers services and routes through `Add{Area}Feature()` and `Map{Area}Endpoints()` in `{Area}FeatureExtensions.cs`. Compose them explicitly in `Program.cs`.

## Responsibilities

### Handlers

Handlers own business logic and feature-specific database access.

- Return expected failures as `Result<T>` or `Result` with a coded `Error.*`, for example `"WorkoutPlans.NotFound"`.
- Do not throw exceptions for expected failures. The global exception handler handles unexpected failures.
- Keep ownership, existence, relationship, and conflict checks in handlers.

### Endpoints

Endpoints only:

- Register routes and bind parameters.
- Apply authorization and endpoint filters.
- Invoke handlers.
- Map `Result<T>` or `Result` to `IResult`.

Apply `ValidationFilter<TRequest>` when request validation is needed.

Do not put business logic or `AppDbContext` access in endpoints or `Program.cs`.

### Validators

Use `{UseCase}Validator : AbstractValidator<TRequest>` for request-shape rules such as required values, lengths, ranges, and basic date rules.

Do not manually validate request shape in handlers. Do not access the database or check ownership in validators.

## Security

- For trainer-owned operations, resolve the authenticated trainer through `ICurrentTrainerAccessor`.
- Enforce ownership by filtering on `trainer.Id` directly or through an already verified owned parent resource.
- Treat route, query, and body IDs only as resource identifiers, never as proof of ownership.
- Require an explicit `TrainerOnly` or `ClientOnly` authorization policy on every protected endpoint.
- Login and registration endpoints under `/api/auth` use `AllowAnonymous`.
- Return `Error.NotFound` for missing and cross-tenant resources to avoid revealing that another trainer's resource exists.
- Load secrets such as `Jwt:SigningKey` from user-secrets or environment variables.
- Never hardcode secrets or log tokens, passwords, or credentials.

## Reference Files

Paths are relative to `src/PersonalTrainer.Api/`.

- Handler: `Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs`
- Endpoint: `Features/WorkoutPlans/AddExercise/AddExerciseEndpoint.cs`
- Validator: `Features/Auth/Register/RegisterValidator.cs`
- Feature registration: `Features/Clients/ClientsFeatureExtensions.cs`
- Results: `Common/Result.cs`
- HTTP result mapping: `Common/ResultHttpExtensions.cs`
- Ownership through a verified parent: `Features/WorkoutPlans/GetWorkoutPlans/GetWorkoutPlansHandler.cs`

## Example

Correct — an ownership-scoped query inside a handler:

```csharp
dbContext.Sessions.Where(s => s.TrainerId == trainer.Id)
```

Not an unscoped query in a route delegate: `app.MapGet("/api/sessions", async (AppDbContext db) => await db.Sessions.ToListAsync())`.

## Verification

Before considering a change complete, run:

```bash
dotnet build
dotnet test --filter "FullyQualifiedName~Unit"
```

Run integration tests matching `FullyQualifiedName~Integration` when a change affects endpoints, authentication, authorization, persistence, migrations, or PostgreSQL-specific behaviour.

Integration tests require Docker.

Report which checks ran and which were skipped. Never claim success without executing the relevant command.

# Personal Trainer API

A minimal, vertical-slice ASP.NET Core (.NET 10) Web API — a backend POC for a Personal Trainer app. Built as a course repo for AI-assisted coding practice.

## Stack

- **.NET 10** minimal API (no MVC controllers)
- **Vertical slice architecture** — each feature lives in its own folder under `Features/`, with a request, handler, validator, and endpoint registration together
- **EF Core + PostgreSQL** (Npgsql)
- **ASP.NET Core Identity + JWT bearer auth**, role-based (`Trainer` / `Client`) with ownership checks in handlers
- **FluentValidation** for request validation
- **xUnit** — unit tests (EF Core InMemory) + integration tests (Testcontainers.PostgreSql + `WebApplicationFactory`)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local Postgres and for running the integration tests, which spin up a real Postgres container via Testcontainers)
  - On Windows, Docker Desktop requires WSL2. If `docker info` fails, run `wsl --install` from an **elevated** PowerShell, then restart Windows.

## Setup

1. **Start local Postgres:**

   ```bash
   docker compose up -d
   ```

   This starts a Postgres 17 container (`personal_trainer` db/user/password — dev-only, non-sensitive) on `localhost:5432`.

2. **Set the JWT signing key** (kept out of source control — user-secrets locally, environment variables elsewhere):

   ```bash
   dotnet user-secrets set "Jwt:SigningKey" "<a long random string, 32+ chars>" --project src/PersonalTrainer.Api
   ```

   Generate one, e.g. in PowerShell:

   ```powershell
   $bytes = New-Object byte[] 48
   (New-Object System.Security.Cryptography.RNGCryptoServiceProvider).GetBytes($bytes)
   [Convert]::ToBase64String($bytes)
   ```

3. **Apply EF Core migrations:**

   ```bash
   dotnet tool install --global dotnet-ef   # if not already installed
   dotnet ef database update --project src/PersonalTrainer.Api --startup-project src/PersonalTrainer.Api
   ```

4. **Run the API:**

   ```bash
   dotnet run --project src/PersonalTrainer.Api
   ```

   In development, an interactive API explorer is available at `/scalar/v1` (backed by the native `/openapi/v1.json` document).

## Testing

```bash
dotnet test
```

- **Unit tests** (`tests/PersonalTrainer.Api.Tests/Unit`) exercise handler business/ownership logic against EF Core's InMemory provider — no external dependencies, fast.
- **Integration tests** (`tests/PersonalTrainer.Api.Tests/Integration`) spin up a real Postgres container per test class via Testcontainers, run the actual migrations, and drive the full HTTP pipeline (registration, login, JWT-authorized requests, cross-tenant ownership rejection). **Requires Docker running.**

## Project layout

```
src/PersonalTrainer.Api/
  Domain/            Entities: Trainer, Client, WorkoutPlan, Exercise, Session, ApplicationUser
  Data/              AppDbContext, EF Core entity configurations, migrations
  Common/            Result<T>, error mapping, current-user/trainer/client accessors,
                     validation filter, global exception handler, security headers
  Auth/              JWT issuing, Identity setup, authorization policies, user provisioning
  Features/          One folder per feature area, one subfolder per vertical slice:
    Auth/            Register, Login
    Clients/         CreateClient, GetClients, GetClientById, UpdateClient, DeleteClient
    WorkoutPlans/     CreateWorkoutPlan, GetWorkoutPlans, AddExercise
    Sessions/        ScheduleSession, GetSessions, UpdateSessionStatus
tests/PersonalTrainer.Api.Tests/
  Unit/              Handler-level tests (EF Core InMemory)
  Integration/        Full HTTP pipeline tests (Testcontainers Postgres)
```

Each slice is self-contained: a request DTO, a `Handler` class (constructor-injected dependencies, single `HandleAsync` method, returns `Result<T>`), a FluentValidation validator where needed, and an `Endpoint` static class that registers a minimal-API route delegate. Feature areas expose `Add<Feature>Feature()` (DI registration) and `Map<Feature>Endpoints()` (route registration) extension methods, composed explicitly in `Program.cs`.

## Domain model

- A **Trainer** owns a roster of **Clients**.
- Each **Client** can have multiple **WorkoutPlans**, each made up of **Exercises**.
- **Sessions** are scheduled trainer/client bookings, optionally linked to a workout plan, with a status (`Scheduled` / `Completed` / `Cancelled` / `NoShow`).
- Every `Trainer` and `Client` is backed by an Identity user account. Registration (`POST /api/auth/register`) is self-service for both roles; trainers can additionally provision client accounts directly (`POST /api/clients`).

## Security notes

- Passwords hashed via ASP.NET Core Identity's default hasher; JWT access tokens are short-lived (60 min) with no refresh token in this POC scope.
- Role-based authorization policies (`TrainerOnly`, `ClientOnly`) plus explicit ownership checks in every handler — a trainer can only see/manage their own clients, plans, and sessions.
- FluentValidation on every mutating request; a global exception handler returns RFC 7807 `ProblemDetails` without leaking internals.
- Fixed-window rate limiting on `/api/auth/*`; explicit CORS allow-list (configured via `Cors:AllowedOrigins`); standard security headers middleware; HTTPS redirection/HSTS.
- No secrets are committed — `Jwt:SigningKey` and the production connection string are supplied via user-secrets (dev) or environment variables (other environments).

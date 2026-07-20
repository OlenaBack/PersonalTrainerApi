# AddExercise slice review

## SUMMARY

This slice lets an authenticated trainer attach one of the trainer's catalog exercises to a trainer-owned workout plan and record plan-specific sets, reps, weight, notes, and order. It verifies ownership, saves a WorkoutPlanExercise join row, and returns the created representation. (src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseHandler.cs:8-58; src/PersonalTrainer.Api/Domain/WorkoutPlanExercise.cs:3-16)

## 1. Responsibility & business requirement

The user-facing capability is attaching a trainer's catalog exercise to a client's workout plan with programming details. AddExerciseRequest carries ExerciseId, Sets, Reps, nullable WeightKg, nullable Notes, and OrderIndex. (src/PersonalTrainer.Api/Features/WorkoutPlans/AddExercise/AddExerciseRequest.cs:3-9)

The handler resolves the current trainer (lines 12-16), requires the plan to match planId and trainer.Id (lines 18-22), requires the exercise to match ExerciseId and trainer.Id (lines 24-29), creates the join entity with the trainer/plan/exercise IDs and request values (lines 31-42), saves it (lines 44-45), and returns its data plus the catalog name and tags (lines 47-57). The route is POST /api/workout-plans/{planId}/exercises and success is 201 Created. (WorkoutPlansFeatureExtensions.cs:23-30; AddExerciseEndpoint.cs:8-24)

## 2. Architecture pattern

The repository uses feature/use-case folders rather than layer folders. A command vertical slice typically contains Request (input contract), Handler (workflow/persistence), Validator (FluentValidation rules), and Endpoint (route, authorization, validation filter, DI, and HTTP mapping). The neighboring CreateExercise slice has exactly CreateExerciseRequest, CreateExerciseHandler, CreateExerciseValidator, and CreateExerciseEndpoint. (src/PersonalTrainer.Api/Features/Exercises/CreateExercise/CreateExerciseRequest.cs:1-3; CreateExerciseHandler.cs:5-33; CreateExerciseValidator.cs:5-11; CreateExerciseEndpoint.cs:6-24)

AddExercise follows that convention: AddExerciseRequest is the input record (AddExerciseRequest.cs:3-9); AddExerciseHandler injects AppDbContext and ICurrentTrainerAccessor (AddExerciseHandler.cs:8-10); AddExerciseValidator defines the request rules (AddExerciseValidator.cs:5-15); AddExerciseEndpoint applies TrainerOnly and ValidationFilter<AddExerciseRequest>, invokes the handler, and maps the result (AddExerciseEndpoint.cs:6-25). WorkoutPlansFeatureExtensions registers the scoped handler/validator and maps the route (WorkoutPlansFeatureExtensions.cs:10-30).

Result<WorkoutPlanExerciseResponse> is an application-level success-or-failure wrapper, not an HTTP response. It exposes Value only on success and carries Error on failure; accessing Value after failure throws. (src/PersonalTrainer.Api/Common/Result.cs:31-53) The endpoint maps success through the Created callback and maps ErrorType values to problem-details status codes. (ResultHttpExtensions.cs:5-26; AddExerciseEndpoint.cs:23-24)

## 3. External dependencies

- Database: AppDbContext.WorkoutPlans, Exercises, and WorkoutPlanExercises are queried/modified by the handler; it uses AnyAsync, SingleOrDefaultAsync, Add, and SaveChangesAsync. (AddExerciseHandler.cs:8-45; src/PersonalTrainer.Api/Data/AppDbContext.cs:8-15)
- Current trainer: ICurrentTrainerAccessor is injected; the default CurrentTrainerAccessor finds the Trainer by current ICurrentUserService.UserId. (src/PersonalTrainer.Api/Common/ICurrentTrainerAccessor.cs:5-7; CurrentTrainerAccessor.cs:7-10)
- DI/feature registration: AddWorkoutPlansFeature registers AddExerciseHandler and IValidator<AddExerciseRequest>; Program.cs enables the feature. (WorkoutPlansFeatureExtensions.cs:10-20; src/PersonalTrainer.Api/Program.cs:24-28)
- Auth: the endpoint requires AuthorizationPolicyNames.TrainerOnly, which requires RoleNames.Trainer. (AddExerciseEndpoint.cs:10-14; src/PersonalTrainer.Api/Auth/AuthenticationExtensions.cs:42-44; RoleNames.cs:3-8)
- Persistence defense: WorkoutPlanExerciseConfiguration uses composite foreign keys (TrainerId, WorkoutPlanId) and (TrainerId, ExerciseId). (src/PersonalTrainer.Api/Data/Configurations/WorkoutPlanExerciseConfiguration.cs:16-30)

## 4. Security considerations

Authorization requires the Trainer role (AddExerciseEndpoint.cs:10-14; AuthenticationExtensions.cs:42-44). No trainer ID is accepted from the client; the handler derives it from the authenticated user via ICurrentTrainerAccessor (AddExerciseHandler.cs:8-16; CurrentTrainerAccessor.cs:7-10). Both the plan and exercise queries include trainer.Id, returning not-found errors for missing or foreign records (AddExerciseHandler.cs:18-29). The join row also stores TrainerId and the EF composite FKs prevent cross-trainer pairings at persistence level (AddExerciseHandler.cs:31-42; WorkoutPlanExerciseConfiguration.cs:16-30).

ValidationFilter runs the registered validator before the handler and returns a validation problem when invalid (src/PersonalTrainer.Api/Common/ValidationFilter.cs:5-31). AddExerciseValidator requires a non-empty ExerciseId, positive Sets/Reps, non-negative optional WeightKg, Notes max length 1000, and non-negative OrderIndex (AddExerciseValidator.cs:5-15).

After endpoint validation, the handler trusts and copies Sets, Reps, WeightKg, Notes, and OrderIndex without a second semantic check (AddExerciseHandler.cs:31-42). There are no visible rules for duplicate exercise membership, order-index collisions, or a maximum weight (AddExerciseValidator.cs:9-14; AddExerciseHandler.cs:24-45). The handler checks plan ownership, while the plan's ClientId is part of the plan entity; it does not perform a separate client query (AddExerciseHandler.cs:18-22; src/PersonalTrainer.Api/Domain/WorkoutPlan.cs:3-16).

## 5. Test coverage

Both types are present. Unit coverage in tests/PersonalTrainer.Api.Tests/Unit/WorkoutPlans/AddExerciseHandlerTests.cs includes success (HandleAsync_OwnPlan_AddsExercise, lines 12-35), foreign plan rejection (lines 37-59), and foreign exercise rejection (lines 61-83). Integration coverage in tests/PersonalTrainer.Api.Tests/Integration/WorkoutPlansEndpointsTests.cs exercises registration/authorization, client/plan/catalog creation, POST AddExercise, 201 Created, and subsequent plan retrieval (CreatePlan_AddExercise_ThenGetPlans_ReturnsPlan, lines 18-46).

No separate AddExercise validator test file was found. The integration test does not cover invalid input or unauthorized/forbidden AddExercise responses, so the repository does not demonstrate every validation and authorization failure path. (the two test files above)

## 6. Main risks (2)

1. Preserve the ownership invariant in both queries and the created row: the current trainer must own the plan and exercise, and TrainerId must be set on the join row. The composite database FKs reinforce but do not replace the explicit checks. (AddExerciseHandler.cs:18-42; WorkoutPlanExerciseConfiguration.cs:16-30)
2. Programming semantics are only partly constrained. New rules for duplicates, order, or weight must be deliberately added to validation/handler/persistence and tests; currently validated request values are persisted directly. (AddExerciseValidator.cs:9-14; AddExerciseHandler.cs:31-45)

## CLOSING SUMMARY

The key invariant is same-trainer ownership of both the workout plan and catalog exercise, enforced in the handler and reinforced by composite database keys. Before changing the slice, preserve authorization and validation wiring and decide explicitly where any new duplicate/order/programming rules belong, then extend both existing unit and integration coverage. (AddExerciseEndpoint.cs:10-24; AddExerciseHandler.cs:12-45; WorkoutPlanExerciseConfiguration.cs:16-30)
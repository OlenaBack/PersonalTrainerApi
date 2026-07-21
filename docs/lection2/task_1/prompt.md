@workspace Add a GetExercises endpoint to a new Exercises feature, mirroring this repo's conventions exactly.

References:
- #file:Domain/Exercise.cs — note: Tags has no explicit EF configuration, so Npgsql maps it to a native Postgres text[] column
- #file:Features/Clients/GetClients/GetClientsRequest.cs and #file:Features/Clients/GetClients/GetClientsHandler.cs — filtering pattern to mirror
- #file:Features/Clients/GetClients/GetClientsEndpoint.cs — endpoint mapping pattern to mirror
- #file:Features/Clients/ClientResponse.cs — response shape convention to mirror
- #file:Features/Clients/ClientsFeatureExtensions.cs — feature registration pattern to mirror

Mirror the file/folder structure of the Clients feature's GetClients slice (Features/Clients/GetClients/ + Features/Clients/ClientResponse.cs + Features/Clients/ClientsFeatureExtensions.cs), adapted 1:1 for an Exercises feature under Features/Exercises/.

Handler requirements:
- Filter by current trainer (ownership check via ICurrentTrainerAccessor, same pattern as reference files)
- The Tags filter parameter must use a type verified to bind correctly from repeated query-string values (?tags=strength&tags=cardio) under ASP.NET Core minimal API [AsParameters] binding. Don't assume any collection type works — confirm the type you choose is one the framework's query binder actually supports for this scenario.
- Only apply the tag filter if Tags is not null and has at least one element — explicit length guard, don't rely on vacuous-true semantics of an empty list
- When applied: match ALL given tags (AND semantics), case-insensitive
- Try to write this so it translates to server-side SQL rather than client-evaluated; if you're not confident this shape translates against Npgsql's provider, say so explicitly instead of silently falling back to an in-memory filter

Wire into Program.cs alongside the other features (using + AddExercisesFeature() + MapExercisesEndpoints()).

Scope: GetExercises only — no CreateExercise, no changes to other features.
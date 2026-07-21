using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Common;

public static class CurrentTrainerAccessorExtensions
{
    private const string TrainerProfileNotFoundSuffix = "TrainerProfileNotFound";

    public static async Task<Result<Trainer>> GetCurrentTrainerOrNotFoundAsync(
        this ICurrentTrainerAccessor currentTrainerAccessor,
        string featurePrefix,
        CancellationToken cancellationToken)
    {
        var trainer = await currentTrainerAccessor.GetCurrentTrainerAsync(cancellationToken);
        if (trainer is null)
        {
            return Error.NotFound($"{featurePrefix}.{TrainerProfileNotFoundSuffix}", "No trainer profile found for the current user.");
        }

        return trainer;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Data.Configurations;

public sealed class WorkoutPlanExerciseConfiguration : IEntityTypeConfiguration<WorkoutPlanExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WeightKg).HasPrecision(6, 2);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        // Tenant is denormalized onto this table so both FKs below can be
        // scoped to (TrainerId, ...), making a cross-trainer pairing of a
        // WorkoutPlan and an Exercise a constraint violation, not just a bug
        // an application-level query has to remember to guard against.
        builder.HasOne(x => x.WorkoutPlan)
            .WithMany(w => w.Exercises)
            .HasForeignKey(x => new { x.TrainerId, x.WorkoutPlanId })
            .HasPrincipalKey(w => new { w.TrainerId, w.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => new { x.TrainerId, x.ExerciseId })
            .HasPrincipalKey(e => new { e.TrainerId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.WorkoutPlanId);
        builder.HasIndex(x => x.ExerciseId);
    }
}

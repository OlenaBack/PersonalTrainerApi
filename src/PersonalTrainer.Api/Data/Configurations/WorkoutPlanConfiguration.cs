using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Data.Configurations;

public sealed class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Title).HasMaxLength(200).IsRequired();
        builder.Property(w => w.Description).HasMaxLength(2000);

        builder.HasOne(w => w.Client)
            .WithMany(c => c.WorkoutPlans)
            .HasForeignKey(w => w.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Trainer)
            .WithMany()
            .HasForeignKey(w => w.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => w.ClientId);
    }
}

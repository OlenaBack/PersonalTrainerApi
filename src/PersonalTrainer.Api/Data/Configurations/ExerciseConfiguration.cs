using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Data.Configurations;

public sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();

        builder.HasAlternateKey(e => new { e.TrainerId, e.Id });

        builder.HasOne(e => e.Trainer)
            .WithMany(t => t.Exercises)
            .HasForeignKey(e => e.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TrainerId);
    }
}

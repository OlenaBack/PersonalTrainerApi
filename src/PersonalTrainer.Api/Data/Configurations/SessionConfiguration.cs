using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalTrainer.Api.Domain;

namespace PersonalTrainer.Api.Data.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Notes).HasMaxLength(1000);

        builder.HasOne(s => s.Client)
            .WithMany(c => c.Sessions)
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Trainer)
            .WithMany()
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.WorkoutPlan)
            .WithMany(w => w.Sessions)
            .HasForeignKey(s => s.WorkoutPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => s.ClientId);
        builder.HasIndex(s => s.TrainerId);
        builder.HasIndex(s => s.ScheduledAtUtc);
    }
}

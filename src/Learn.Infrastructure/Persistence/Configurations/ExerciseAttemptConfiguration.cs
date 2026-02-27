using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class ExerciseAttemptConfiguration : IEntityTypeConfiguration<ExerciseAttempt>
{
    public void Configure(EntityTypeBuilder<ExerciseAttempt> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(e => e.UserAnswer)
            .HasMaxLength(10000)
            .IsRequired();

        builder.Property(e => e.Feedback)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(e => e.SuggestedCorrection)
            .HasMaxLength(5000);

        builder.Property(e => e.DetailedBreakdown)
            .HasMaxLength(10000);

        builder.HasIndex(e => new { e.UserId, e.ExerciseId });

        builder.HasOne(e => e.Exercise)
            .WithMany(ex => ex.Attempts)
            .HasForeignKey(e => e.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

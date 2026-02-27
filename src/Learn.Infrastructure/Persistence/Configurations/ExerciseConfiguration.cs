using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Prompt)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.Context)
            .HasMaxLength(5000);

        builder.Property(e => e.ReferenceAnswer)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(e => e.AudioUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Hints)
            .HasMaxLength(2000);

        builder.Property(e => e.ExerciseType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.DifficultyLevel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(e => new { e.LessonId, e.OrderIndex });

        builder.HasOne(e => e.QuestionBankItem)
            .WithMany(q => q.Exercises)
            .HasForeignKey(e => e.QuestionBankItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.DailyLesson)
            .WithMany(d => d.Exercises)
            .HasForeignKey(e => e.DailyLessonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Attempts)
            .WithOne(a => a.Exercise)
            .HasForeignKey(a => a.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

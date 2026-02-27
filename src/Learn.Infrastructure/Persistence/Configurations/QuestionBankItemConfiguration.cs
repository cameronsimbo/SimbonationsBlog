using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class QuestionBankItemConfiguration : IEntityTypeConfiguration<QuestionBankItem>
{
    public void Configure(EntityTypeBuilder<QuestionBankItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(e => e.Prompt)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.Context)
            .HasMaxLength(5000);

        builder.Property(e => e.ReferenceAnswer)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(e => e.Hints)
            .HasMaxLength(2000);

        builder.Property(e => e.SubjectDomain)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.ExerciseType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.DifficultyLevel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(e => new { e.UserId, e.SubjectDomain });

        builder.HasMany(e => e.Exercises)
            .WithOne(ex => ex.QuestionBankItem)
            .HasForeignKey(ex => ex.QuestionBankItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

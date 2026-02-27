using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(t => t.IconUrl)
            .HasMaxLength(500);

        builder.Property(t => t.SubjectDomain)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.DifficultyLevel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.KeyConcepts)
            .HasMaxLength(2000);

        builder.Property(t => t.GenerationGuidance)
            .HasMaxLength(2000);

        builder.HasMany(t => t.Units)
            .WithOne(u => u.Topic)
            .HasForeignKey(u => u.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

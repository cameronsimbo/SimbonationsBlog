using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class DailyLessonConfiguration : IEntityTypeConfiguration<DailyLesson>
{
    public void Configure(EntityTypeBuilder<DailyLesson> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(e => new { e.UserId, e.LessonDate })
            .IsUnique();

        builder.HasOne(e => e.Topic)
            .WithMany()
            .HasForeignKey(e => e.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Exercises)
            .WithOne(ex => ex.DailyLesson)
            .HasForeignKey(ex => ex.DailyLessonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
